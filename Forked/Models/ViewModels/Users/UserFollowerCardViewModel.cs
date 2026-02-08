using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Users
{
    public class UserFollowerCardViewModel
    {
        public int ID {  get; set; }
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        public string? ProfilePicturePath { get; set; }

    }
}
