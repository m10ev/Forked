using Forked.Models.Interfaces;

namespace Forked.Models.Domains
{
    public class Recipe : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ImagePaths { get; set; } = new List<string>();
        public int? PreparationTimeInMinutes { get; set; }
        public int? CookingTimeInMinutes { get; set; }
        public int Servings { get; set; }
        public string AuthorId { get; set; } = null!;
        public User Author { get; set; } = null!;
        public int ParentRecipeId { get; set; }
        public Recipe? ParentRecipe { get; set; }
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<RecipeStep> RecipeSteps { get; set; } = new List<RecipeStep>();
        public ICollection<UserFavoriteRecipe> FavoritedByUsers { get; set; } = new List<UserFavoriteRecipe>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Recipe> Forks { get; set; } = new List<Recipe>();
    }
}
