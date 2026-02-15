using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Recipes
{
    public class EditRecipeViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public List<string> ImagePaths { get; set; } = new();
        public List<IFormFile>? ImageFiles { get; set; }

        public int? PreparationTimeInMinutes { get; set; }
        public int? CookingTimeInMinutes { get; set; }

        [Required]
        public int Servings { get; set; }

        [Required]
        public List<ParsedIngredientViewModel> ParsedIngredients { get; set; } = new();

        [Required]
        public List<EditRecipeStepViewModel> Steps { get; set; } = new();
    }

}
