using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Forked.Models.ViewModels.Recipes
{
    public class RecipeStepViewModel
    {
        public int Id { get; set; }
        public int StepNumber { get; set; }
        public string? StepName { get; set; }
        public string Instruction { get; set; } = string.Empty;
        public List<string> ImagePaths { get; set; } = new();

        public string DisplayTitle => string.IsNullOrWhiteSpace(StepName)
            ? $"Step {StepNumber}"
            : $"{StepNumber}. {StepName}";
    }
}
