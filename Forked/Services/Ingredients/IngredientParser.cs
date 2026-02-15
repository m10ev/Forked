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
            if (words.Count > 0)
            {
                var word = words[0].ToLower();

                var singularUnit = _units.FirstOrDefault(u => u == word)
                                   ?? _units.FirstOrDefault(u => word == u.Pluralize()); // check plural -> singular

                if (singularUnit != null)
                {
                    result.Unit = singularUnit;
                    words.RemoveAt(0);
                }
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
            // Replace dash with space if present
            input = input.Replace("-", " ");

            var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            decimal total = 0;

            // First token: whole number
            if (tokens.Length > 0 && decimal.TryParse(tokens[0], out var whole))
            {
                total += whole;
            }
            else
            {
                // maybe a word number like "one"
                var words = new Dictionary<string, decimal>
        {
            { "a", 1 }, { "an", 1 },
            { "one", 1 }, { "two", 2 }, { "three", 3 }
        };
                if (tokens.Length > 0 && words.TryGetValue(tokens[0], out var wordVal))
                    total += wordVal;
            }

            // Second token: fraction
            if (tokens.Length > 1 && tokens[1].Contains("/"))
            {
                var parts = tokens[1].Split('/');
                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out var numerator) &&
                    decimal.TryParse(parts[1], out var denominator))
                {
                    total += numerator / denominator;
                }
            }
            else if (tokens.Length == 1 && tokens[0].Contains("/")) // single fraction
            {
                var parts = tokens[0].Split('/');
                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out var numerator) &&
                    decimal.TryParse(parts[1], out var denominator))
                {
                    total += numerator / denominator;
                }
            }

            return total;
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
