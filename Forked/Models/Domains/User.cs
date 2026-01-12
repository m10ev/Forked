using Microsoft.AspNetCore.Identity;
using Forked.Models.Interfaces;

namespace Forked.Models.Domains
{
    public class User : IdentityUser, IAuditable
    {
        public string Username { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public string? Bio { get; set; }
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<UserFavoriteRecipe> FavoriteRecipes { get; set; } = new List<UserFavoriteRecipe>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
