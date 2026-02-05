using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Recipes
{
    public class RecipeIngredientViewModel
    {
        [Required(ErrorMessage = "Ingredient name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Ingredient name must be between 2 and 200 characters")]
        [Display(Name = "Ingredient Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, 10000, ErrorMessage = "Quantity must be between 0.01 and 10000")]
        [Display(Name = "Quantity")]
        public double Quantity { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Unit must be between 1 and 50 characters")]
        [Display(Name = "Unit")]
        public string Unit { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Preparation cannot exceed 200 characters")]
        [Display(Name = "Preparation (optional)")]
        public string Preparation { get; set; } = string.Empty;

        [BindNever]
        public string DisplayText => string.IsNullOrWhiteSpace(Preparation)
            ? $"{Quantity} {Unit} {Name}"
            : $"{Quantity} {Unit} {Name}, {Preparation}";
    }
}
