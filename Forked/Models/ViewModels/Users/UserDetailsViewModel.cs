using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Users
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        [Display(Name = "Username")]
        public string DisplayName { get; set; } = string.Empty;
        [Display(Name = "Bio")]
        public string Description {  get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        [Display(Name = "Followers")]
        public int FollowersCount { get; set; } 
        [Display(Name = "Following")]
        public int FollowingCount { get; set; } 
       // [Display(Name = "Forked")]
       // public int ForkedCount { get; set; }
        [Display(Name = "Recipes")]
        public int RecipeCount { get; set; } 
    }
}
