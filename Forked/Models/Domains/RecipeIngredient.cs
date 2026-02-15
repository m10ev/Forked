using Forked.Models.Interfaces;

namespace Forked.Models.Domains
{
    public class RecipeIngredient : BaseEntity
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; } = null!;
        public double Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Preparation { get; set; } = string.Empty;
    }
}
