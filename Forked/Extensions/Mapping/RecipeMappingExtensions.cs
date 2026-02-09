using Forked.Data;
using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;
using Forked.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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


        public static async Task<List<RecipeIngredient>> ToRecipeIngredientsAsync(
            this List<CreateRecipeIngredientViewModel> ingredientViewModels,
            ForkedDbContext context)
        {
            var ingredientNames = ingredientViewModels
                .Select(i => i.Name.Trim())
                .Distinct()
                .ToList();

            // Get existing ingredients
            var existing = await context.Ingredients
                .Where(i => ingredientNames.Contains(i.Name))
                .ToListAsync();

            // Create new ingredients that don't exist
            var newIngredients = ingredientViewModels
                .Where(i => !existing.Any(e => e.Name.Equals(i.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                .Select(i => new Ingredient { Name = i.Name.Trim() })
                .DistinctBy(i => i.Name)
                .ToList();

            context.Ingredients.AddRange(newIngredients);
            await context.SaveChangesAsync();

            // Map to RecipeIngredients
            return ingredientViewModels.Select(i =>
            {
                var ingredient = existing.FirstOrDefault(e => e.Name.Equals(i.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                                 ?? newIngredients.First(e => e.Name.Equals(i.Name.Trim(), StringComparison.OrdinalIgnoreCase));

                return new RecipeIngredient
                {
                    Ingredient = ingredient,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Preparation = i.Preparation
                };
            }).ToList();
        }

        public static async Task<List<RecipeStep>> ToRecipeStepsAsync(
            this List<CreateRecipeStepViewModel> stepViewModels,
            IImageService imageService)
        {
            var steps = new List<RecipeStep>();

            foreach (var stepVm in stepViewModels)
            {
                var step = new RecipeStep
                {
                    StepNumber = stepVm.StepNumber,
                    StepName = stepVm.StepName,
                    Instruction = stepVm.Instruction,
                    ImagePaths = new List<string>()
                };

                if (stepVm.ImageFiles != null && stepVm.ImageFiles.Any())
                {
                    foreach (var file in stepVm.ImageFiles)
                    {
                        var path = await imageService.SaveStepImageAsync(file);
                        step.ImagePaths.Add(path);
                    }
                }

                steps.Add(step);
            }

            return steps.OrderBy(s => s.StepNumber).ToList();
        }
    }
}