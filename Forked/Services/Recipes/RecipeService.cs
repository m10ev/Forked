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
            Recipe recipe = vm.ToRecipe(authorId);

            if (parentRecipeId.HasValue)
            {
                recipe.ParentRecipeId = parentRecipeId.Value;
            }

            recipe.RecipeSteps = await MapStepsAsync(vm, _imageService);

            foreach (var step in recipe.RecipeSteps)
            {
                step.Recipe = recipe;
            }

            recipe.RecipeIngredients = await MapIngredientsAsync(vm);

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return recipe;
        }

        private async Task<List<RecipeIngredient>> MapIngredientsAsync(CreateRecipeViewModel vm)
        {
            var ingredientNames = vm.Ingredients.Select(i => i.Name.Trim()).Distinct().ToList();

            var existing = await _context.Ingredients
                .Where(i => ingredientNames.Contains(i.Name))
                .ToListAsync();

            var newIngredients = vm.Ingredients
                .Where(i => !existing.Any(e => e.Name == i.Name))
                .Select(i => new Ingredient { Name = i.Name })
                .ToList();

            _context.Ingredients.AddRange(newIngredients);
            await _context.SaveChangesAsync();

            return vm.Ingredients.Select(i =>
            {
                var ingredient = existing.FirstOrDefault(e => e.Name == i.Name)
                                 ?? newIngredients.First(e => e.Name == i.Name);

                return new RecipeIngredient
                {
                    Ingredient = ingredient,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Preparation = i.Preparation
                };
            }).ToList();
        }

        private async Task<List<RecipeStep>> MapStepsAsync(CreateRecipeViewModel vm, IImageService imageService)
        {
            var steps = new List<RecipeStep>();

            for (int i = 0; i < vm.Steps.Count; i++)
            {
                var stepVm = vm.Steps[i];

                var step = new RecipeStep
                {
                    StepNumber = stepVm.StepNumber,
                    StepName = stepVm.StepName,
                    Instruction = stepVm.Instruction,
                    ImagePaths = new List<string>()
                };

                if (stepVm.ImageFiles != null)
                {
                    foreach (var file in stepVm.ImageFiles)
                    {
                        var path = await imageService.SaveStepImageAsync(file);
                        step.ImagePaths.Add(path);
                    }
                }

                steps.Add(step);
            }

            // Sort by step number
            return steps.OrderBy(s => s.StepNumber).ToList();
        }



        public Task DeleteRecipeAsync(int id, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
