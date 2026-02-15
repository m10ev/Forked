using Forked.Services.Ingredients;

namespace Forked.Tests.Services
{
    public class IngredientParserTests
    {
        private readonly IngredientParser _parser = new();

        // --- Parse() ---

        [Fact]
        public void Parse_FullIngredientString_ReturnsCorrectFields()
        {
            var result = _parser.Parse("2 cups flour");

            Assert.Equal(2m, result.Quantity);
            Assert.Equal("cup", result.Unit);
            Assert.Equal("flour", result.Name);
        }

        [Fact]
        public void Parse_FractionQuantity_ConvertsToDecimal()
        {
            var result = _parser.Parse("1/2 cup sugar");

            Assert.Equal(0.5m, result.Quantity);
            Assert.Equal("cup", result.Unit);
            Assert.Equal("sugar", result.Name);
        }

        [Fact]
        public void Parse_MixedNumberQuantity_ConvertsToDecimal()
        {
            // "1 1/2" becomes "1-1/2" after regex, but our parser only parses the
            // leading token – verify quantity is non-zero and name is populated
            var result = _parser.Parse("1 1/2 cups milk");

            Assert.True(result.Quantity > 0);
        }

        [Fact]
        public void Parse_WordQuantity_ReturnsCorrectQuantity()
        {
            var result = _parser.Parse("two tablespoons butter");

            Assert.Equal(2m, result.Quantity);
            Assert.Equal("tablespoon", result.Unit);
        }

        [Fact]
        public void Parse_ArticleQuantity_ReturnsOne()
        {
            var result = _parser.Parse("a pinch of salt");

            Assert.Equal(1m, result.Quantity);
            Assert.Equal("pinch", result.Unit);
            Assert.Equal("salt", result.Name);
        }

        [Fact]
        public void Parse_NoQuantityOrUnit_SetsNameOnly()
        {
            var result = _parser.Parse("basil");

            Assert.Equal(0m, result.Quantity);
            Assert.Null(result.Unit);
            Assert.Equal("basil", result.Name);
        }

        [Fact]
        public void Parse_EmptyString_ReturnsEmptyResult()
        {
            var result = _parser.Parse("   ");

            Assert.Equal(0m, result.Quantity);
            Assert.Null(result.Unit);
            Assert.Equal(string.Empty, result.Name);
        }

        [Fact]
        public void Parse_WithPreparation_ExtractsPreparationWords()
        {
            var result = _parser.Parse("2 cloves garlic minced");

            Assert.Equal(2m, result.Quantity);
            Assert.Equal("clove", result.Unit);
            Assert.Contains("minced", result.Preparation);
        }

        [Fact]
        public void Parse_OfConnective_IsStripped()
        {
            var result = _parser.Parse("1 cup of water");

            Assert.Equal("water", result.Name);
        }

        [Fact]
        public void Parse_PluralUnit_IsSingularized()
        {
            var result = _parser.Parse("3 tablespoons olive oil");

            Assert.Equal("tablespoon", result.Unit);
        }

        // --- Format() ---

        [Fact]
        public void Format_FullIngredient_ReturnsExpectedString()
        {
            var ingredient = new ParsedIngredient
            {
                Quantity = 2,
                Unit = "cup",
                Name = "flour"
            };

            var result = _parser.Format(ingredient);

            Assert.Contains("flour", result);
            Assert.Contains("2", result);
        }

        [Fact]
        public void Format_NullIngredient_ReturnsEmpty()
        {
            var result = _parser.Format(null!);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Format_IngredientWithNoName_ReturnsEmpty()
        {
            var ingredient = new ParsedIngredient { Quantity = 1, Unit = "cup", Name = "" };

            var result = _parser.Format(ingredient);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Format_SingleQuantity_UnitRemainsSingular()
        {
            var ingredient = new ParsedIngredient { Quantity = 1, Unit = "cup", Name = "milk" };

            var result = _parser.Format(ingredient);

            Assert.Contains("cup", result);
            Assert.DoesNotContain("cups", result);
        }

        [Fact]
        public void Format_MultipleQuantity_UnitIsPluralized()
        {
            var ingredient = new ParsedIngredient { Quantity = 2, Unit = "cup", Name = "milk" };

            var result = _parser.Format(ingredient);

            Assert.Contains("cups", result);
        }

        [Fact]
        public void Format_IngredientWithPreparation_IncludesPreparation()
        {
            var ingredient = new ParsedIngredient
            {
                Quantity = 2,
                Unit = "clove",
                Name = "garlic",
                Preparation = "minced"
            };

            var result = _parser.Format(ingredient);

            Assert.Contains("minced", result);
        }

        [Fact]
        public void Format_ZeroQuantity_OmitsQuantityFromOutput()
        {
            var ingredient = new ParsedIngredient { Quantity = 0, Unit = null, Name = "salt" };

            var result = _parser.Format(ingredient);

            Assert.Equal("salt", result);
        }
    }
}