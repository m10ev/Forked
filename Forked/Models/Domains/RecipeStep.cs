using Forked.Models.Interfaces;

namespace Forked.Models.Domains
{
    public class RecipeStep : BaseEntity
    {
        public int StepNumber { get; set; }
        public string? StepName { get; set; }
        public string Instruction { get; set; } = string.Empty;
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        public List<string> ImagePaths { get; set; } = new List<string>();
    }
}
