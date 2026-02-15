using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Forked.Models.Domains;

namespace Forked.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public IndexModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        // Base64 cropped image from modal
        [BindProperty]
        public string? CroppedProfilePictureBase64 { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Display Name")]
            public string DisplayName { get; set; } = string.Empty;

            [Display(Name = "Bio")]
            [StringLength(500)]
            public string? Bio { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }

            // Only for displaying the preview
            public string? ProfilePicturePath { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var phone = await _userManager.GetPhoneNumberAsync(user);
            Input = new InputModel
            {
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                PhoneNumber = phone,
                ProfilePicturePath = user.ProfilePicturePath
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            bool updated = false;

            // DisplayName
            if (user.DisplayName != Input.DisplayName)
            {
                user.DisplayName = Input.DisplayName;
                updated = true;
            }

            // Bio
            if (user.Bio != Input.Bio)
            {
                user.Bio = Input.Bio;
                updated = true;
            }

            // PhoneNumber
            var phone = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phone)
            {
                var result = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Failed to update phone number.");
                    return Page();
                }
                updated = true;
            }

            // Profile picture
            if (!string.IsNullOrEmpty(CroppedProfilePictureBase64))
            {
                try
                {
                    var base64Data = CroppedProfilePictureBase64.Split(',')[1];
                    var bytes = Convert.FromBase64String(base64Data);

                    // Delete old picture
                    if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    // Save new picture
                    var fileName = $"{Guid.NewGuid()}.png";
                    var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                    Directory.CreateDirectory(saveDir);

                    var fullPath = Path.Combine(saveDir, fileName);
                    await System.IO.File.WriteAllBytesAsync(fullPath, bytes);

                    user.ProfilePicturePath = $"/images/profiles/{fileName}";
                    updated = true;
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Failed to save profile picture.");
                    return Page();
                }
            }

            if (updated)
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Failed to update profile.");
                    return Page();
                }

                await _signInManager.RefreshSignInAsync(user);
            }

            StatusMessage = "Profile updated successfully!";
            return RedirectToPage();
        }
    }
}
