using Microsoft.AspNetCore.Identity;

namespace Forked.Models.Domains
{
    public class User : IdentityUser
    {
        public string Username { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public string? Bio { get; set; }
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<UserFavoriteRecipe> FavoriteRecipes { get; set; } = new List<UserFavoriteRecipe>();
    }
}
