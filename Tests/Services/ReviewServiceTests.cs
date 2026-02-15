using Forked.Data;
using Forked.Models.Domains;
using Forked.Models.ViewModels.Reviews;
using Forked.Services.Interfaces;
using Forked.Services.Reviews;
using Moq;
using Tests;

namespace Tests.Services
{
    public class ReviewServiceTests
    {
        private readonly Mock<IImageService> _imageServiceMock = new();

        private ReviewService CreateService(ForkedDbContext ctx) =>
            new(ctx, _imageServiceMock.Object);

        // --- AddReviewAsync ---

        [Fact]
        public async Task AddReviewAsync_NullViewModel_ThrowsArgumentNullException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                svc.AddReviewAsync(null!, "user1"));
        }

        [Fact]
        public async Task AddReviewAsync_RatingBelowRange_ThrowsArgumentOutOfRangeException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);
            var vm = new CreateReviewViewModel { Rating = 0, Message = "ok", RecipeId = 1 };

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                svc.AddReviewAsync(vm, "user1"));
        }

        [Fact]
        public async Task AddReviewAsync_RatingAboveRange_ThrowsArgumentOutOfRangeException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);
            var vm = new CreateReviewViewModel { Rating = 6, Message = "ok", RecipeId = 1 };

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                svc.AddReviewAsync(vm, "user1"));
        }

        [Fact]
        public async Task AddReviewAsync_EmptyMessage_ThrowsArgumentException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);
            var vm = new CreateReviewViewModel { Rating = 3, Message = "   ", RecipeId = 1 };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.AddReviewAsync(vm, "user1"));
        }

        // --- GetReviewForEditAsync ---

        [Fact]
        public async Task GetReviewForEditAsync_ReviewNotFound_ReturnsNull()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            var result = await svc.GetReviewForEditAsync(999, "user1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetReviewForEditAsync_DifferentUser_ThrowsUnauthorizedAccessException()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Reviews.Add(new Review { Id = 1, UserId = "owner", RecipeId = 1, Rating = 4, Message = "Good" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                svc.GetReviewForEditAsync(1, "intruder"));
        }

        [Fact]
        public async Task GetReviewForEditAsync_OwnerAccess_ReturnsViewModel()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Reviews.Add(new Review { Id = 2, UserId = "owner", RecipeId = 1, Rating = 5, Message = "Amazing" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var result = await svc.GetReviewForEditAsync(2, "owner");

            Assert.NotNull(result);
        }

        // --- UpdateReviewAsync ---

        [Fact]
        public async Task UpdateReviewAsync_NullViewModel_ThrowsArgumentNullException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                svc.UpdateReviewAsync(null!, "user1"));
        }

        [Fact]
        public async Task UpdateReviewAsync_ReviewNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);
            var vm = new UpdateReviewViewModel { Id = 999 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.UpdateReviewAsync(vm, "user1"));
        }

        [Fact]
        public async Task UpdateReviewAsync_DifferentUser_ThrowsUnauthorizedAccessException()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Reviews.Add(new Review { Id = 3, UserId = "owner", RecipeId = 1, Rating = 3, Message = "Okay" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var vm = new UpdateReviewViewModel { Id = 3 };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                svc.UpdateReviewAsync(vm, "intruder"));
        }

        // --- GetPagedReviewsAsync ---

        [Fact]
        public async Task GetPagedReviewsAsync_PageBelowOne_ClampsToOne()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            var result = await svc.GetPagedReviewsAsync(recipeId: 1, currentUserId: null, page: -5, pageSize: 5);

            Assert.Equal(1, result.Paging.CurrentPage);
        }

        [Fact]
        public async Task GetPagedReviewsAsync_PageSizeAbove100_ClampsTo100()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            var result = await svc.GetPagedReviewsAsync(recipeId: 1, currentUserId: null, page: 1, pageSize: 9999);

            Assert.Equal(100, result.Paging.PageSize);
        }

        [Fact]
        public async Task GetPagedReviewsAsync_NoReviews_ReturnsEmptyList()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            var result = await svc.GetPagedReviewsAsync(recipeId: 42, currentUserId: null);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.Paging.TotalItems);
        }

        [Fact]
        public async Task GetPagedReviewsAsync_HasReviews_ReturnsPaginatedResults()
        {
            using var ctx = DbContextFactory.Create();

            for(int i = 1; i <= 7; i++)
            {
                ctx.Users.Add(new User { Id = $"u{i}", UserName = $"user{i}" });
            }

            ctx.Recipes.Add(new Recipe
            {
                Id = 1,
                Title = "Test Recipe",
                Description = "Desc",
                AuthorId = "u1"
            });

            for (int i = 1; i <= 7; i++)
            {
                ctx.Reviews.Add(new Review
                {
                    RecipeId = 1,
                    UserId = "u1",
                    Rating = 3,
                    Message = $"Review {i}",
                    CreatedAt = DateTime.UtcNow.AddMinutes(i)
                });
            }

            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);

            var page1 = await svc.GetPagedReviewsAsync(recipeId: 1, currentUserId: null, page: 1, pageSize: 5);
            var page2 = await svc.GetPagedReviewsAsync(recipeId: 1, currentUserId: null, page: 2, pageSize: 5);

            Assert.Equal(7, page1.Paging.TotalItems);
            Assert.Equal(5, page1.Items.Count());
            Assert.Equal(2, page2.Items.Count());
        }


        // --- DeleteReviewAsync ---

        [Fact]
        public async Task DeleteReviewAsync_ReviewNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.DeleteReviewAsync(999, "user1", "User"));
        }

        [Fact]
        public async Task DeleteReviewAsync_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Reviews.Add(new Review { Id = 4, UserId = "owner", RecipeId = 1, Rating = 4, Message = "Nice" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                svc.DeleteReviewAsync(4, "intruder", "User"));
        }

        [Fact]
        public async Task DeleteReviewAsync_AdminUser_CanDeleteAnyReview()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Reviews.Add(new Review { Id = 5, UserId = "owner", RecipeId = 1, Rating = 4, Message = "Nice" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);

            // Should not throw
            await svc.DeleteReviewAsync(5, "admin_user", "Admin");

            Assert.Empty(ctx.Reviews);
        }

        [Fact]
        public async Task DeleteReviewAsync_Owner_CanDeleteOwnReview()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Reviews.Add(new Review { Id = 6, UserId = "owner", RecipeId = 1, Rating = 2, Message = "Meh" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.DeleteReviewAsync(6, "owner", "User");

            Assert.Empty(ctx.Reviews);
        }
    }

}
