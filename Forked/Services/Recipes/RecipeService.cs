using Forked.Data;
using Forked.Extensions.Mapping;
using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;
using Forked.Services.Interfaces;

namespace Forked.Services.Recipes
{
    public class RecipeService : IRecipeService
    {
        private readonly ForkedDbContext _context;
        private readonly IImageService _imageService;

        public RecipeService(ForkedDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<Recipe> CreateAsync(CreateRecipeViewModel vm, string authorId, int? parentRecipeId = null)
        {
            Recipe recipe = vm.ToRecipe(authorId);

            if (parentRecipeId.HasValue)
            {
                recipe.ParentRecipeId = parentRecipeId.Value;
            }

            recipe.RecipeSteps = await vm.Steps.ToRecipeStepsAsync(_imageService);
            foreach (var step in recipe.RecipeSteps)
            {
                step.Recipe = recipe;
            }

            recipe.RecipeIngredients = await vm.Ingredients.ToRecipeIngredientsAsync(_context);

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return recipe;
        }

        public Task DeleteRecipeAsync(int id, string userId)
        {
            throw new NotImplementedException();
        }
    }
}