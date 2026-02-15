namespace Forked.Services.Ingredients
{
    public class ParsedIngredient
    {
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Preparation { get; set; }
    }
}
