using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Recipes
{
    public class EditRecipeStepViewModel
    {
        public int? Id { get; set; }   // null = new step

        [Required]
        [Range(1, 100)]
        public int StepNumber { get; set; }

        [StringLength(100)]
        public string? StepName { get; set; }

        [Required]
        [StringLength(2000)]
        public string Instruction { get; set; } = string.Empty;

        public List<string> ImagePaths { get; set; } = new();
        public List<IFormFile>? ImageFiles { get; set; }
    }

}
