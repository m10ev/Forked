using Forked.Models.Interfaces;

namespace Forked.Models.Domains
{
    public class Recipe : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public int? PreparationTimeInMinutes { get; set; }
        public int? CookingTimeInMinutes { get; set; }
        public int Servings { get; set; }
        public string? AuthorId { get; set; }
        public User? Author { get; set; }
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<UserFavoriteRecipe> FavoritedByUsers { get; set; } = new List<UserFavoriteRecipe>();
    }
}
