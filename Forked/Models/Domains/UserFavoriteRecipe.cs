namespace Forked.Models.Domains
{
    public class UserFavoriteRecipe
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;
    }
}
