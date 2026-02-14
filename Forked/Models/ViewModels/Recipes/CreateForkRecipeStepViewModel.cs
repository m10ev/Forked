using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Recipes
{
    public class CreateForkRecipeStepViewModel
    {
        [Required(ErrorMessage = "Step number is required")]
        [Range(1, 100, ErrorMessage = "Step number must be between 1 and 100")]
        [Display(Name = "Step Number")]
        public int StepNumber { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Step name must be between 3 and 100 characters")]
        [Display(Name = "Step Name (optional)")]
        public string? StepName { get; set; }

        [Required(ErrorMessage = "Step instruction is required")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Instruction must be between 5 and 2000 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Instruction")]
        public string Instruction { get; set; } = string.Empty;

        [Display(Name = "Step Images (optional)")]
        public List<string>? ImagePaths { get; set; }

        [Display(Name = "Step Images (optional)s")]
        [DataType(DataType.Upload)]
        public List<IFormFile> ImageFiles { get; set; } = new();

        [BindNever]
        public string InstructionPreview
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Instruction))
                    return string.Empty;

                return Instruction.Length > 50
                    ? Instruction.Substring(0, 50) + "..."
                    : Instruction;
            }
        }

        [BindNever]
        public string DisplayTitle => string.IsNullOrWhiteSpace(StepName)
            ? $"Step {StepNumber}"
            : $"{StepNumber}. {StepName}";
    }
}
