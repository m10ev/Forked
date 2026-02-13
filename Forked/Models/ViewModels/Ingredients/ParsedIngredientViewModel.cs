using System.ComponentModel.DataAnnotations;

public class ParsedIngredientViewModel
{
    public double Quantity { get; set; }
    public string? Unit { get; set; } = string.Empty;
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Preparation { get; set; } = string.Empty;
}
