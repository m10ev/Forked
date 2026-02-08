using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Users
{
    public class UserFollowerCardViewModel
    {
        public string Id { get; set; } = string.Empty;
        [Display(Name = "Username")]
        public string DisplayName { get; set; } = string.Empty;
        [Display(Name = "Bio")]
        public string Description { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }

    }
}
