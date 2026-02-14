using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;

namespace Forked.Services.Recipes
{
    public interface IRecipeService
    {
        Task<Recipe> CreateAsync(CreateRecipeViewModel vm, string authorId, int? parentRecipeId = null);
        Task<CreateForkViewModel> PrepareForkAsync(int originalRecipeId);
        Task<Recipe> ForkAsync(CreateForkViewModel vm, string authorId);
            Task<RecipeDetailViewModel?> GetRecipeDetailAsync(int id, string? currentUserId);
        Task<RecipeListViewModel> GetPagedRecipesAsync(RecipeFilterViewModel filters, RecipeSortOption sortBy, int page, int pageSize, string? currentUserId);
        Task DeleteRecipeAsync(int id, string userId, bool isAdmin);
    }
}
