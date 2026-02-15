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

        public static async Task<CreateForkViewModel> PrepareForkAsync(this Recipe original)
        {
            var vm = new CreateForkViewModel
            {
                Title = original.Title,
                Description = original.Description,
                Servings = original.Servings,
                PreparationTimeInMinutes = original.PreparationTimeInMinutes,
                CookingTimeInMinutes = original.CookingTimeInMinutes,
                ParentRecipeId = original.Id,
                ImagePaths = original.ImagePaths,
                Steps = original.RecipeSteps
                    .OrderBy(s => s.StepNumber)
                    .Select(s => new CreateForkRecipeStepViewModel
                    {
                        StepNumber = s.StepNumber,
                        StepName = s.StepName,
                        Instruction = s.Instruction,
                        ImagePaths = s.ImagePaths
                    }).ToList(),
                ParsedIngredients = original.RecipeIngredients
                    .Select(ri => new ParsedIngredientViewModel
                    {
                        Quantity = ri.Quantity,
                        Unit = ri.Unit,
                        Name = ri.Ingredient?.Name,
                        Preparation = ri.Preparation
                    }).ToList()
            };

            return vm;
        }

        public static async Task<Recipe> ToRecipeAsync(this CreateForkViewModel viewModel, string userId, IImageService imageService)
        {
            var recipe = new Recipe
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                PreparationTimeInMinutes = viewModel.PreparationTimeInMinutes,
                CookingTimeInMinutes = viewModel.CookingTimeInMinutes,
                Servings = viewModel.Servings,
                AuthorId = userId,
                ParentRecipeId = viewModel.ParentRecipeId,
                ImagePaths = viewModel.ImagePaths ?? new()
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

        public static async Task<List<RecipeStep>> ToRecipeStepsAsync(
            this List<CreateForkRecipeStepViewModel> stepViewModels,
            IImageService imageService)
        {
            var steps = new List<RecipeStep>();

            foreach (var stepVm in stepViewModels)
            {
                var step = new RecipeStep
                {
                    StepNumber = stepVm.StepNumber,
                    StepName = stepVm.StepName,
                    Instruction = stepVm.Instruction
                };

                step.ImagePaths.AddRange(stepVm.ImagePaths ?? new List<string>());

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

        public static EditRecipeViewModel ToEditViewModel(this Recipe recipe)
        {
            return new EditRecipeViewModel
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                PreparationTimeInMinutes = recipe.PreparationTimeInMinutes,
                CookingTimeInMinutes = recipe.CookingTimeInMinutes,
                Servings = recipe.Servings,
                ImagePaths = recipe.ImagePaths ?? new(),

                ParsedIngredients = recipe.RecipeIngredients
                    .Select(ri => new ParsedIngredientViewModel
                    {
                        Quantity = ri.Quantity,
                        Unit = ri.Unit,
                        Name = ri.Ingredient?.Name,
                        Preparation = ri.Preparation
                    }).ToList(),

                Steps = recipe.RecipeSteps
                    .OrderBy(s => s.StepNumber)
                    .Select(s => new EditRecipeStepViewModel
                    {
                        Id = s.Id,  // IMPORTANT for diff-based update
                        StepNumber = s.StepNumber,
                        StepName = s.StepName,
                        Instruction = s.Instruction,
                        ImagePaths = s.ImagePaths ?? new()
                    }).ToList()
            };
        }


        public static async Task UpdateFromViewModelAsync(
    this Recipe recipe,
    EditRecipeViewModel vm,
    ForkedDbContext context,
    IImageService imageService)
        {
            // 1. Scalars
            recipe.Title = vm.Title;
            recipe.Description = vm.Description;
            recipe.Servings = vm.Servings;
            recipe.PreparationTimeInMinutes = vm.PreparationTimeInMinutes;
            recipe.CookingTimeInMinutes = vm.CookingTimeInMinutes;

            // 2. Recipe Images
            var vmImagePaths = vm.ImagePaths ?? new List<string>();
            var removedImages = recipe.ImagePaths
                .Where(img => !vmImagePaths.Contains(img))
                .ToList();

            foreach (var img in removedImages)
                await imageService.DeleteAsync(img);

            recipe.ImagePaths = vmImagePaths.ToList();

            if (vm.ImageFiles?.Any() == true)
            {
                foreach (var file in vm.ImageFiles)
                {
                    var path = await imageService.SaveRecipeImageAsync(file);
                    recipe.ImagePaths.Add(path);
                }
            }

            // 3. Ingredients (SYNC PATTERN - Stops the Duplicates)
            var incomingData = await vm.ParsedIngredients.ToRecipeIngredientsAsync(context);
            var currentRecipeIngredients = recipe.RecipeIngredients.ToList();

            foreach (var existingRI in currentRecipeIngredients)
            {
                // MATCH: Check by ID or Name (Case-Insensitive)
                var match = incomingData.FirstOrDefault(i =>
                    (i.IngredientId != 0 && i.IngredientId == existingRI.IngredientId) ||
                    (existingRI.Ingredient != null && i.Ingredient != null &&
                     string.Equals(i.Ingredient.Name?.Trim(), existingRI.Ingredient.Name?.Trim(), StringComparison.OrdinalIgnoreCase)));

                if (match != null)
                {
                    // SYNC: Update the existing row
                    existingRI.Quantity = match.Quantity;
                    existingRI.Unit = match.Unit;
                    existingRI.Preparation = match.Preparation;
                    existingRI.DeletedAt = null; // Reactivate if it was soft-deleted

                    context.Entry(existingRI).State = EntityState.Modified;

                    // Remove from incoming so we don't ADD it again later
                    incomingData.Remove(match);
                }
                else
                {
                    // SOFT-DELETE: Mark as deleted to hide from queries
                    if (existingRI.DeletedAt == null)
                    {
                        existingRI.DeletedAt = DateTime.UtcNow;
                        context.Entry(existingRI).State = EntityState.Modified;
                    }
                }
            }

            // ADD TRULY NEW: Anything left was not matched to an existing record
            foreach (var brandNew in incomingData)
            {
                recipe.RecipeIngredients.Add(brandNew);
            }

            // 4. Steps (SYNC PATTERN)
            var existingSteps = recipe.RecipeSteps.ToList();
            var vmSteps = vm.Steps ?? new List<EditRecipeStepViewModel>();

            // Soft-delete removed steps
            foreach (var existing in existingSteps.Where(s => s.DeletedAt == null))
            {
                if (!vmSteps.Any(s => s.Id == existing.Id))
                {
                    if (existing.ImagePaths != null)
                    {
                        foreach (var img in existing.ImagePaths)
                            await imageService.DeleteAsync(img);
                    }
                    existing.DeletedAt = DateTime.UtcNow;
                    context.Entry(existing).State = EntityState.Modified;
                }
            }

            foreach (var stepVm in vmSteps)
            {
                if (stepVm.Id.HasValue && stepVm.Id.Value != 0)
                {
                    var existing = existingSteps.FirstOrDefault(s => s.Id == stepVm.Id.Value);
                    if (existing == null) continue;

                    existing.DeletedAt = null;
                    existing.StepNumber = stepVm.StepNumber;
                    existing.StepName = stepVm.StepName;
                    existing.Instruction = stepVm.Instruction;

                    // Step image diff
                    var vmStepPaths = stepVm.ImagePaths ?? new List<string>();
                    var removedStepImages = (existing.ImagePaths ?? new List<string>())
                        .Where(img => !vmStepPaths.Contains(img))
                        .ToList();

                    foreach (var img in removedStepImages)
                        await imageService.DeleteAsync(img);

                    existing.ImagePaths = vmStepPaths.ToList();

                    if (stepVm.ImageFiles?.Any() == true)
                    {
                        foreach (var file in stepVm.ImageFiles)
                        {
                            var path = await imageService.SaveStepImageAsync(file);
                            existing.ImagePaths.Add(path);
                        }
                    }
                }
                else
                {
                    // Truly New Step
                    var newStep = new RecipeStep
                    {
                        StepNumber = stepVm.StepNumber,
                        StepName = stepVm.StepName,
                        Instruction = stepVm.Instruction,
                        RecipeId = recipe.Id,
                        ImagePaths = new List<string>()
                    };

                    if (stepVm.ImageFiles?.Any() == true)
                    {
                        foreach (var file in stepVm.ImageFiles)
                        {
                            var path = await imageService.SaveStepImageAsync(file);
                            newStep.ImagePaths.Add(path);
                        }
                    }
                    recipe.RecipeSteps.Add(newStep);
                }
            }
        }

    }
}
