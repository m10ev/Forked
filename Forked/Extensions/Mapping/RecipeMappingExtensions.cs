using Forked.Data;
using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;
using Forked.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Forked.Extensions.Mapping
{
    public static class RecipeMappingExtensions
    {
        public static async Task<Recipe> ToRecipe(this CreateRecipeViewModel viewModel, string userId, IImageService imageService)
        {
            var recipe = new Recipe
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                PreparationTimeInMinutes = viewModel.PreparationTimeInMinutes,
                CookingTimeInMinutes = viewModel.CookingTimeInMinutes,
                Servings = viewModel.Servings,
                AuthorId = userId,
                ImagePaths = new()
            };

            if (viewModel.ImageFiles?.Any() == true)
            {
                foreach (var image in viewModel.ImageFiles)
                {
                    var path = await imageService.SaveRecipeImageAsync(image);
                    recipe.ImagePaths.Add(path);
                }
            }

            return recipe;
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
                AuthorName = recipe.Author?.DisplayName ?? "Unknown",
                IsAuthor = currentUserId == recipe.AuthorId,

                ParentRecipeId = recipe.ParentRecipeId,
                ParentRecipeTitle = recipe.ParentRecipe?.Title,
                ParentRecipeAuthorName = recipe.ParentRecipe?.Author?.DisplayName,
                ForkCount = recipe.Forks?.Count ?? 0,

                ReviewCount = recipe.Reviews?.Count ?? 0,
                AverageRating = recipe.Reviews != null && recipe.Reviews.Any()
                    ? Math.Round(recipe.Reviews.Average(r => r.Rating), 2)
                    : 0,

                HasUserReviewed = !string.IsNullOrEmpty(currentUserId) &&
                                  recipe.Reviews?.Any(r => r.UserId == currentUserId) == true,

                UserReviewId = recipe.Reviews?
                    .FirstOrDefault(r => r.UserId == currentUserId)?.Id,

                IsFavourite = !string.IsNullOrEmpty(currentUserId) &&
                              recipe.FavoritedByUsers?.Any(f => f.UserId == currentUserId) == true,

                Ingredients = recipe.RecipeIngredients
                    .Select(ri => ri.ToViewModel())
                    .ToList(),

                Steps = recipe.RecipeSteps
                    .OrderBy(s => s.StepNumber)
                    .Select(s => s.ToViewModel())
                    .ToList(),

                Reviews = recipe.Reviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.ToViewModel(currentUserId))
                    .ToList()
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
                ImagePaths = recipeStep.ImagePaths ?? new()
            };
        }

        public static RecipeCardViewModel ToCardViewModel(this Recipe recipe, string? currentUserId = null)
        {
            return new RecipeCardViewModel
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                ImagePath = recipe.ImagePaths.FirstOrDefault(),
                TotalTimeInMinutes = (recipe.PreparationTimeInMinutes ?? 0) + (recipe.CookingTimeInMinutes ?? 0),
                Servings = recipe.Servings,
                AuthorName = recipe.Author?.DisplayName ?? "Unknown",
                AverageRating = recipe.Reviews != null && recipe.Reviews.Any()
                    ? Math.Round(recipe.Reviews.Average(x => x.Rating), 1)
                    : 0,
                ReviewCount = recipe.Reviews?.Count ?? 0,
                IsForked = recipe.ParentRecipeId != null,
                IsFavoritedByCurrentUser = !string.IsNullOrEmpty(currentUserId)
                    && recipe.FavoritedByUsers != null
                    && recipe.FavoritedByUsers.Any(f => f.UserId == currentUserId),
                CreatedAt = recipe.CreatedAt
            };
        }

        public static async Task<List<RecipeIngredient>> ToRecipeIngredientsAsync(
                this List<ParsedIngredientViewModel> parsedIngredients,
                ForkedDbContext context)
        {
            var normalizedNames = parsedIngredients
                .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                .Select(i => i.Name.Trim().ToLower())
                .Distinct()
                .ToList();

            var existingIngredients = await context.Ingredients
                .Where(i => normalizedNames.Contains(i.Name.ToLower()))
                .ToListAsync();

            var ingredientDict = existingIngredients
                .ToDictionary(i => i.Name.ToLower(), i => i);

            var recipeIngredients = new List<RecipeIngredient>();

            foreach (var parsed in parsedIngredients)
            {
                if (string.IsNullOrWhiteSpace(parsed.Name))
                    continue;

                var normalized = parsed.Name.Trim().ToLower();

                if (!ingredientDict.TryGetValue(normalized, out var ingredientEntity))
                {
                    ingredientEntity = new Ingredient
                    {
                        Name = CultureInfo.CurrentCulture.TextInfo
                            .ToTitleCase(parsed.Name.Trim().ToLower())
                    };
                    context.Ingredients.Add(ingredientEntity);
                    ingredientDict[normalized] = ingredientEntity;
                }

                recipeIngredients.Add(new RecipeIngredient
                {
                    Ingredient = ingredientEntity,
                    Quantity = parsed.Quantity,
                    Unit = parsed.Unit ?? string.Empty,
                    Preparation = parsed.Preparation ?? string.Empty
                });
            }

            return recipeIngredients;
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
                    ImagePaths = new()
                };

                if (stepVm.ImageFiles?.Any() == true)
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
