using Forked.Data;
using Forked.Extensions.Mapping;
using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;
using Forked.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            Recipe recipe = await vm.ToRecipe(authorId, _imageService);

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

        public async Task<RecipeDetailViewModel?> GetRecipeDetailAsync(int id, string? currentUserId)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Author)
                .Include(r => r.ParentRecipe)
                    .ThenInclude(p => p.Author)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeSteps)
                .Include(r => r.Reviews)
                    .ThenInclude(rev => rev.User)
                .Include(r => r.Forks)
                .Include(r => r.FavoritedByUsers)
                .FirstOrDefaultAsync(r => r.Id == id);

            return recipe?.ToDetailViewModel(currentUserId);
        }

        public async Task DeleteRecipeAsync(int id, string userId)
        {
            var recipe = await _context.Recipes.FindAsync(id);

            if (recipe == null)
                throw new KeyNotFoundException("Recipe not found");

            if (recipe.AuthorId != userId)
                throw new UnauthorizedAccessException("You can only delete your own recipes");

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }
    }
}