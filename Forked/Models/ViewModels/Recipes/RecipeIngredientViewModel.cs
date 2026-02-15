namespace Forked.Models.ViewModels.Recipes
{
    public class RecipeIngredientViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Preparation { get; set; } = string.Empty;

        public string DisplayText => string.IsNullOrWhiteSpace(Preparation)
            ? $"{Quantity} {Unit} {Name}"
            : $"{Quantity} {Unit} {Name}, {Preparation}";
    }
}
