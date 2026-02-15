namespace Forked.Services.Ingredients
{
    public interface IIngredientParser
    {
        ParsedIngredient Parse(string input);
        string Format(ParsedIngredient ingredient);
    }
}
