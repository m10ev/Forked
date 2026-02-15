using Forked.Data;
using Forked.Models.Domains;
using Forked.Services.UserFavoriteRecipes;
using Forked.Services.Users;
using Microsoft.EntityFrameworkCore;
using Tests;


namespace Tests.Services
{
    public class FavoriteServiceTests
    {
        private static FavoriteService CreateService(ForkedDbContext ctx) => new(ctx);

        // --- AddFavouriteAsync ---

        [Fact]
        public async Task AddFavouriteAsync_NewFavourite_AddsRecord()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            await svc.AddFavouriteAsync("user1", 1);

            Assert.Single(ctx.UserFavoriteRecipes);
        }

        [Fact]
        public async Task AddFavouriteAsync_Duplicate_DoesNotAddSecondRecord()
        {
            using var ctx = DbContextFactory.Create();
            ctx.UserFavoriteRecipes.Add(new UserFavoriteRecipe { UserId = "user1", RecipeId = 1 });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.AddFavouriteAsync("user1", 1);

            Assert.Single(ctx.UserFavoriteRecipes);
        }

        [Fact]
        public async Task AddFavouriteAsync_RestoresSoftDeleted_InsteadOfAddingNew()
        {
            using var ctx = DbContextFactory.Create();
            // Simulate a soft-deleted record (IgnoreQueryFilters will find it)
            var deleted = new UserFavoriteRecipe
            {
                UserId = "user1",
                RecipeId = 2,
                DeletedAt = DateTime.UtcNow
            };
            ctx.UserFavoriteRecipes.Add(deleted);
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.AddFavouriteAsync("user1", 2);

            // Only one record should exist (restored, not duplicated)
            var all = await ctx.UserFavoriteRecipes.IgnoreQueryFilters().ToListAsync();
            Assert.Single(all);
            Assert.Null(all[0].DeletedAt);
        }

        // --- RemoveFavouriteAsync ---

        [Fact]
        public async Task RemoveFavouriteAsync_ExistingFavourite_RemovesRecord()
        {
            using var ctx = DbContextFactory.Create();
            ctx.UserFavoriteRecipes.Add(new UserFavoriteRecipe { UserId = "user1", RecipeId = 3 });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.RemoveFavouriteAsync("user1", 3);

            Assert.Empty(ctx.UserFavoriteRecipes);
        }

        [Fact]
        public async Task RemoveFavouriteAsync_NonExistentFavourite_DoesNotThrow()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            // Should complete without throwing
            await svc.RemoveFavouriteAsync("user1", 999);
        }

        // --- IsFavouriteAsync ---

        [Fact]
        public async Task IsFavouriteAsync_FavouriteExists_ReturnsTrue()
        {
            using var ctx = DbContextFactory.Create();
            ctx.UserFavoriteRecipes.Add(new UserFavoriteRecipe { UserId = "user1", RecipeId = 4 });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var result = await svc.IsFavouriteAsync("user1", 4);

            Assert.True(result);
        }

        [Fact]
        public async Task IsFavouriteAsync_FavouriteDoesNotExist_ReturnsFalse()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            var result = await svc.IsFavouriteAsync("user1", 999);

            Assert.False(result);
        }

        [Fact]
        public async Task IsFavouriteAsync_DifferentUser_ReturnsFalse()
        {
            using var ctx = DbContextFactory.Create();
            ctx.UserFavoriteRecipes.Add(new UserFavoriteRecipe { UserId = "user1", RecipeId = 5 });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var result = await svc.IsFavouriteAsync("user2", 5);

            Assert.False(result);
        }
    }

    // ============================================================
    //  UserService Tests
    // ============================================================
    public class UserServiceTests
    {
        private static UserService CreateService(ForkedDbContext ctx) => new(ctx);

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_ValidUser_AddsAndReturnsUser()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);
            var user = new User { Id = "u1", DisplayName = "Alice", Email = "alice@example.com" };

            var result = await svc.CreateAsync(user);

            Assert.Equal("Alice", result.DisplayName);
            Assert.Single(ctx.Users);
        }

        // --- DeleteAsync ---

        [Fact]
        public async Task DeleteAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.DeleteAsync("ghost"));
        }

        [Fact]
        public async Task DeleteAsync_ExistingUser_RemovesUser()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Users.Add(new User { Id = "u2", DisplayName = "Bob", Email = "bob@example.com" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.DeleteAsync("u2");

            Assert.Empty(ctx.Users);
        }

        // --- GetUserCardAsync ---

        [Fact]
        public async Task GetUserCardAsync_UserNotFound_ReturnsNull()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            var result = await svc.GetUserCardAsync("nobody");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserCardAsync_UserExists_ReturnsViewModel()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Users.Add(new User { Id = "u3", DisplayName = "Carol", Email = "carol@example.com" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var result = await svc.GetUserCardAsync("Carol");

            Assert.NotNull(result);
        }

        // --- GetUserDetailsAsync ---

        [Fact]
        public async Task GetUserDetailsAsync_UserNotFound_ReturnsNull()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            var result = await svc.GetUserDetailsAsync("nobody", null);

            Assert.Null(result);
        }

        // --- FollowAsync ---

        [Fact]
        public async Task FollowAsync_TargetNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Users.Add(new User { Id = "u4", DisplayName = "Dave", Email = "dave@example.com" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.FollowAsync("u4", "NonExistent"));
        }

        [Fact]
        public async Task FollowAsync_ValidUsers_CreatesFollowRecord()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Users.Add(new User { Id = "u5", DisplayName = "Eve", Email = "eve@example.com" });
            ctx.Users.Add(new User { Id = "u6", DisplayName = "Frank", Email = "frank@example.com" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.FollowAsync("u5", "Frank");

            Assert.Single(ctx.UserFollows);
        }

        [Fact]
        public async Task FollowAsync_AlreadyFollowing_DoesNotCreateDuplicate()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Users.Add(new User { Id = "u7", DisplayName = "Grace", Email = "grace@example.com" });
            ctx.Users.Add(new User { Id = "u8", DisplayName = "Hank", Email = "hank@example.com" });
            ctx.UserFollows.Add(new UserFollow { FollowerId = "u7", FollowingId = "u8" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.FollowAsync("u7", "Hank");

            Assert.Single(ctx.UserFollows);
        }

        // --- UnfollowAsync ---

        [Fact]
        public async Task UnfollowAsync_TargetNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Users.Add(new User { Id = "u9", DisplayName = "Iris", Email = "iris@example.com" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.UnfollowAsync("u9", "Ghost"));
        }

        [Fact]
        public async Task UnfollowAsync_ExistingFollow_RemovesRecord()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Users.Add(new User { Id = "u10", DisplayName = "Jack", Email = "j@example.com" });
            ctx.Users.Add(new User { Id = "u11", DisplayName = "Kim", Email = "k@example.com" });
            ctx.UserFollows.Add(new UserFollow { FollowerId = "u10", FollowingId = "u11" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.UnfollowAsync("u10", "Kim");

            Assert.Empty(ctx.UserFollows);
        }

        [Fact]
        public async Task UnfollowAsync_NoExistingFollow_DoesNotThrow()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Users.Add(new User { Id = "u12", DisplayName = "Leo", Email = "l@example.com" });
            ctx.Users.Add(new User { Id = "u13", DisplayName = "Mia", Email = "m@example.com" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);

            // Should complete without throwing
            await svc.UnfollowAsync("u12", "Mia");
        }
    }

}
