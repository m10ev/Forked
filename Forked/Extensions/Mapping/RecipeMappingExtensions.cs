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

        public static RecipeDetailViewModel ToDetailViewModel(this Recipe recipe, string? currentUserId = null)
        {
            return new RecipeDetailViewModel
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                ImagePaths = recipe.ImagePaths,
                PreparationTimeInMinutes = recipe.PreparationTimeInMinutes,
                CookingTimeInMinutes = recipe.CookingTimeInMinutes,
                Servings = recipe.Servings,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt,

                AuthorId = recipe.AuthorId,
                AuthorName = recipe.Author?.UserName ?? "Unknown",
                IsAuthor = currentUserId != null && recipe.AuthorId == currentUserId,

                ParentRecipeId = recipe.ParentRecipeId,
                ParentRecipeTitle = recipe.ParentRecipe?.Title,
                ParentRecipeAuthorName = recipe.ParentRecipe?.Author?.UserName,
                ForkCount = recipe.Forks?.Count ?? 0,

                ReviewCount = recipe.Reviews?.Count ?? 0,
                AverageRating = recipe.Reviews != null && recipe.Reviews.Any()
                        ? Math.Round(recipe.Reviews.Average(r => r.Rating), 2)
                        : 0,
                HasUserReviewed = !string.IsNullOrEmpty(currentUserId) && recipe.Reviews != null && recipe.Reviews.Any(r => r.UserId == currentUserId),
                UserReviewId = recipe.Reviews?.FirstOrDefault(r => r.UserId == currentUserId)?.Id,

                IsFavourite = !string.IsNullOrEmpty(currentUserId) && (recipe.FavoritedByUsers?.Any(f => f.UserId == currentUserId) ?? false),

                Ingredients = recipe.RecipeIngredients?
                    .Select(ri => ri.ToViewModel())
                    .ToList() ?? new(),
                Steps = recipe.RecipeSteps?
                    .OrderBy(s => s.StepNumber)
                    .Select(s => s.ToViewModel())
                    .ToList() ?? new(),
                Reviews = recipe.Reviews?
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.ToViewModel(currentUserId))
                    .ToList() ?? new()
            };
        }

        public static RecipeIngredientViewModel ToViewModel(this RecipeIngredient recipeIngredient)
        {
            return new RecipeIngredientViewModel
            {
                Id = recipeIngredient.Id,
                Name = recipeIngredient.Ingredient?.Name ?? "Unknown",
                Quantity = recipeIngredient.Quantity,
                Unit = recipeIngredient.Unit,
                Preparation = recipeIngredient.Preparation ?? string.Empty
            };
        }

        public static RecipeStepViewModel ToViewModel(this RecipeStep recipeStep)
        {
            return new RecipeStepViewModel
            {
                Id = recipeStep.Id,
                StepNumber = recipeStep.StepNumber,
                StepName = recipeStep.StepName,
                Instruction = recipeStep.Instruction,
                ImagePaths = recipeStep.ImagePaths ?? new List<string>()
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