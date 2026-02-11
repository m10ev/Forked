using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Recipes
{
    public class CreateRecipeViewModel
    {
        [Required(ErrorMessage = "Recipe title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        [Display(Name = "Recipe Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Recipe Images")]
        [DataType(DataType.Upload)]
        public List<IFormFile> ImageFiles { get; set; } = new();

        [Display(Name = "Preparation Time (minutes)")]
        [Range(1, 1440, ErrorMessage = "Preparation time must be between 1 and 1440 minutes")]
        public int? PreparationTimeInMinutes { get; set; }

        [Display(Name = "Cooking Time (minutes)")]
        [Range(1, 1440, ErrorMessage = "Cooking time must be between 1 and 1440 minutes")]
        public int? CookingTimeInMinutes { get; set; }

        [Required(ErrorMessage = "Number of servings is required")]
        [Range(1, 100, ErrorMessage = "Servings must be between 1 and 100")]
        [Display(Name = "Servings")]
        public int Servings { get; set; }

        [Display(Name = "Ingredients")]
        [Required(ErrorMessage = "At least one ingredient is required")]
        [MinLength(1, ErrorMessage = "At least one ingredient is required")]
        public List<CreateRecipeIngredientViewModel> Ingredients { get; set; } = new List<CreateRecipeIngredientViewModel>();

        [Display(Name = "Recipe Steps")]
        [Required(ErrorMessage = "At least one recipe step is required")]
        [MinLength(1, ErrorMessage = "At least one recipe step is required")]
        public List<CreateRecipeStepViewModel> Steps { get; set; } = new List<CreateRecipeStepViewModel>();

        [BindNever]
        public int? TotalTimeInMinutes
        {
            get
            {
                if (!PreparationTimeInMinutes.HasValue && !CookingTimeInMinutes.HasValue)
                    return null;

                return (PreparationTimeInMinutes ?? 0) + (CookingTimeInMinutes ?? 0);
            }
        }

        [BindNever]
        public bool HasIngredients => Ingredients != null && Ingredients.Count > 0 &&
            Ingredients.Any(i => !string.IsNullOrWhiteSpace(i.Name));

        [BindNever]
        public bool HasSteps => Steps != null && Steps.Count > 0 &&
            Steps.Any(s => !string.IsNullOrWhiteSpace(s.Instruction));
    }
}