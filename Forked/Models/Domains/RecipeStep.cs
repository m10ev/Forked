namespace Forked.Models.Domains
{
    public class RecipeStep
    {
        public int Id { get; set; }

        public int StepNumber { get; set; }

        public string StepName { get; set; } = string.Empty;
        public string Instruction { get; set; } = string.Empty;
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;

        public List<string> ImagePaths { get; set; } = new();

    }
}
