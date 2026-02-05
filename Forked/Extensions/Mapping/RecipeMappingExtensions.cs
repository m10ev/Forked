using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;

namespace Forked.Extensions.Mapping
{
    public static class RecipeMappingExtensions
    {
        public static Recipe ToRecipe(this CreateRecipeViewModel viewModel, string userId)
        {
            return new Recipe
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                PreparationTimeInMinutes = viewModel.PreparationTimeInMinutes,
                CookingTimeInMinutes = viewModel.CookingTimeInMinutes,
                Servings = viewModel.Servings,
                AuthorId = userId
            };
        }
    }

}
