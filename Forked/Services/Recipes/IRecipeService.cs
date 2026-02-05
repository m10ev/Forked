using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;

namespace Forked.Services.Recipes
{
    public interface IRecipeService
    {
        Task<Recipe> CreateAsync(CreateRecipeViewModel vm, string authorId, int? parentRecipeId = null);
        Task DeleteRecipeAsync(int id, string userId);
    }
}
