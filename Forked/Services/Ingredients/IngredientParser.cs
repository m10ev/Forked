using System.Text.RegularExpressions;
using Humanizer;

namespace Forked.Services.Ingredients
{
    public class IngredientParser : IIngredientParser
    {
        private readonly List<string> _units = new()
        {
            "cup","tbsp","tablespoon","tsp","teaspoon","oz","ounce",
            "ml","l","g","kg","lb","pound","clove","can","jar",
            "package","bag","bottle","pinch","dash","sprig","slice","piece"
        };

        public ParsedIngredient Parse(string input)
        {
            var result = new ParsedIngredient();
            var working = input.ToLower().Trim();

            working = Regex.Replace(working, @"(\d+)\s+(\d\/\d+)", "$1-$2");

            var words = working.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (words.Count == 0)
                return result;

            // Quantity
            result.Quantity = ConvertToDecimal(words[0]);
            if (result.Quantity > 0)
                words.RemoveAt(0);

            // Unit
            if (words.Count > 0 && _units.Contains(words[0].Singularize()))
            {
                result.Unit = words[0].Singularize();
                words.RemoveAt(0);
            }

            // Remove "of"
            if (words.Count > 0 && words[0] == "of")
                words.RemoveAt(0);

            // Remaining words = name / preparation
            var nameParts = new List<string>();
            var prepParts = new List<string>();

            foreach (var word in words)
            {
                if (word.EndsWith("ed"))
                    prepParts.Add(word);
                else
                    nameParts.Add(word);
            }

            result.Name = string.Join(" ", nameParts).Singularize();
            result.Preparation = string.Join(" ", prepParts);

            return result;
        }

        private decimal ConvertToDecimal(string input)
        {
            if (decimal.TryParse(input, out var val))
                return val;

            if (input.Contains("/"))
            {
                var parts = input.Split('/');
                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out var n) &&
                    decimal.TryParse(parts[1], out var d))
                    return n / d;
            }

            var words = new Dictionary<string, decimal>
            {
                { "a", 1 }, { "an", 1 },
                { "one", 1 }, { "two", 2 }, { "three", 3 }
            };

            if (words.TryGetValue(input, out var wordVal))
                return wordVal;

            return 0;
        }

        public string Format(ParsedIngredient ingredient)
        {
            if (ingredient == null || string.IsNullOrWhiteSpace(ingredient.Name))
                return string.Empty;

            var parts = new List<string>();

            if (ingredient.Quantity > 0)
                parts.Add(ingredient.Quantity.ToString("0.##"));

            if (!string.IsNullOrWhiteSpace(ingredient.Unit))
                parts.Add(PluralizeUnit(ingredient.Unit, ingredient.Quantity));

            parts.Add(ingredient.Name);

            if (!string.IsNullOrWhiteSpace(ingredient.Preparation))
                parts.Add($", {ingredient.Preparation}");

            return string.Join(" ", parts);
        }

        private string PluralizeUnit(string unit, decimal amount)
        {
            if (amount == 1)
                return unit;

            return unit.Pluralize();
        }


    }
}
