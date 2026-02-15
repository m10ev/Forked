using Forked.Models.Interfaces;

namespace Forked.Models.Domains
{
    public class UserFavoriteRecipe : IAuditable
    {
        public string UserId { get; set; }
        public User User { get; set; } = null!;
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        public DateTime CreatedAt {  get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt {  get; set; }
        public DateTime? DeletedAt {  get; set; }
    }
}
