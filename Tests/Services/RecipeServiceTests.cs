using Forked.Data;
using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;
using Forked.Services.Ingredients;
using Forked.Services.Interfaces;
using Forked.Services.Recipes;
using Moq;
using Tests;


namespace Tests.Services
{

    public class RecipeServiceTests
    {
        private readonly Mock<IImageService> _imageServiceMock = new();

        private RecipeService CreateService(ForkedDbContext ctx) =>
            new(ctx, _imageServiceMock.Object);

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_NullViewModel_ThrowsArgumentNullException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                svc.CreateAsync(null!, "author1"));
        }

        [Fact]
        public async Task CreateAsync_NullParsedIngredients_ThrowsArgumentException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);
            var vm = new CreateRecipeViewModel
            {
                ParsedIngredients = null
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.CreateAsync(vm, "author1"));
        }

        [Fact]
        public async Task CreateAsync_EmptyParsedIngredients_ThrowsArgumentException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);
            var vm = new CreateRecipeViewModel
            {
                ParsedIngredients = new List<ParsedIngredientViewModel>()
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.CreateAsync(vm, "author1"));
        }

        // --- ForkAsync ---

        [Fact]
        public async Task ForkAsync_NullViewModel_ThrowsArgumentNullException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                svc.ForkAsync(null!, "author1"));
        }

        [Fact]
        public async Task ForkAsync_NullIngredients_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);
            var vm = new CreateForkViewModel
            {
                ParsedIngredients = null,
                Steps = new List<CreateForkRecipeStepViewModel> { new() }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.ForkAsync(vm, "author1"));
        }

        [Fact]
        public async Task ForkAsync_EmptySteps_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);
            var vm = new CreateForkViewModel
            {
                ParsedIngredients = new List<ParsedIngredientViewModel> { new() { Name = "salt" } },
                Steps = new List<CreateForkRecipeStepViewModel>()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.ForkAsync(vm, "author1"));
        }

        // --- PrepareForkAsync ---

        [Fact]
        public async Task PrepareForkAsync_RecipeNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.PrepareForkAsync(9999));
        }

        // --- GetRecipeDetailAsync ---

        [Fact]
        public async Task GetRecipeDetailAsync_RecipeNotFound_ReturnsNull()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            var result = await svc.GetRecipeDetailAsync(9999, null);

            Assert.Null(result);
        }

        // --- GetRecipeForEditAsync ---

        [Fact]
        public async Task GetRecipeForEditAsync_RecipeNotFound_ReturnsNull()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            var result = await svc.GetRecipeForEditAsync(9999, "user1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetRecipeForEditAsync_WrongUser_ThrowsUnauthorizedAccessException()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Recipes.Add(new Recipe { Id = 1, AuthorId = "owner", Title = "Pasta", Description = "Desc" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                svc.GetRecipeForEditAsync(1, "intruder"));
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_RecipeNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);
            var vm = new EditRecipeViewModel
            {
                Id = 9999,
                ParsedIngredients = new List<ParsedIngredientViewModel> { new() { Name = "flour" } },
                Steps = new List<EditRecipeStepViewModel> { new() }
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.UpdateAsync(vm, "user1"));
        }

        [Fact]
        public async Task UpdateAsync_WrongUser_ThrowsUnauthorizedAccessException()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Recipes.Add(new Recipe { Id = 2, AuthorId = "owner", Title = "Soup", Description = "Desc" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var vm = new EditRecipeViewModel
            {
                Id = 2,
                ParsedIngredients = new List<ParsedIngredientViewModel> { new() { Name = "water" } },
                Steps = new List<EditRecipeStepViewModel> { new() }
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                svc.UpdateAsync(vm, "intruder"));
        }

        [Fact]
        public async Task UpdateAsync_EmptyIngredients_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Recipes.Add(new Recipe { Id = 3, AuthorId = "owner", Title = "Stew", Description = "Desc" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var vm = new EditRecipeViewModel
            {
                Id = 3,
                ParsedIngredients = new List<ParsedIngredientViewModel>(),
                Steps = new List<EditRecipeStepViewModel> { new() }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.UpdateAsync(vm, "owner"));
        }

        [Fact]
        public async Task UpdateAsync_EmptySteps_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Recipes.Add(new Recipe { Id = 4, AuthorId = "owner", Title = "Cake", Description = "Desc" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var vm = new EditRecipeViewModel
            {
                Id = 4,
                ParsedIngredients = new List<ParsedIngredientViewModel> { new() { Name = "flour" } },
                Steps = new List<EditRecipeStepViewModel>()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.UpdateAsync(vm, "owner"));
        }

        // --- DeleteRecipeAsync ---

        [Fact]
        public async Task DeleteRecipeAsync_RecipeNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.DeleteRecipeAsync(9999, "user1", false));
        }

        [Fact]
        public async Task DeleteRecipeAsync_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Recipes.Add(new Recipe { Id = 5, AuthorId = "owner", Title = "Pizza", Description = "Desc" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                svc.DeleteRecipeAsync(5, "intruder", false));
        }

        [Fact]
        public async Task DeleteRecipeAsync_AdminUser_CanDeleteAnyRecipe()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Recipes.Add(new Recipe { Id = 6, AuthorId = "owner", Title = "Tacos", Description = "Desc" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.DeleteRecipeAsync(6, "admin", isAdmin: true);

            Assert.Empty(ctx.Recipes);
        }

        [Fact]
        public async Task DeleteRecipeAsync_Owner_CanDeleteOwnRecipe()
        {
            using var ctx = DbContextFactory.Create();
            ctx.Recipes.Add(new Recipe { Id = 7, AuthorId = "owner", Title = "Sushi", Description = "Desc" });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.DeleteRecipeAsync(7, "owner", isAdmin: false);

            Assert.Empty(ctx.Recipes);
        }
    }
}
