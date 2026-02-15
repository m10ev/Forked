using Forked.Models.Domains;
using Forked.Services.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Forked.Controllers
{
    [Route("Users")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService; 
        private readonly UserManager<User> _userManager; 
        public UsersController(IUserService userService, UserManager<User> userManager)
        {
            _userService = userService;
            _userManager = userManager; 
        }

        // GET: /User/Details/{displayName}
        [HttpGet("Details/{displayName}")]
        public async Task<IActionResult> Details(string displayName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id;
            var userDetails = await _userService.GetUserDetailsAsync(displayName, currentUserId);
            if (userDetails == null)
            {
                return NotFound();
            }

            return View(userDetails);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsMe()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            return RedirectToAction("Details", new { displayName = currentUser.DisplayName });
        }

        // GET: /User/Followers/{displayName}
        [HttpGet("Followers/{displayName}")]
        public async Task<IActionResult> Followers(string displayName, int page = 1, int pageSize = 12) 
        {
            var followers = await _userService.GetFollowersAsync(displayName, page, pageSize);
            return View(followers); 
        }

        // GET: /User/Following/{displayName}
        [HttpGet("Following/{displayName}")]
        public async Task<IActionResult> Following(string displayName, int page = 1, int pageSize = 12) 
        {
            var following = await _userService.GetFollowingAsync(displayName, page, pageSize);

            return View(following);
        }

        // GET: /User/Card/{displayName}
        [HttpGet("Card/{displayName}")]
        public async Task<IActionResult> Card(string displayName) 
        { 
            var card = await _userService.GetUserCardAsync(displayName);
            if (card == null)
            {
                return NotFound();
            }

            return PartialView("_UserCard", card);
        }

        [HttpPost("Follow/{displayName}")]
        public async Task<IActionResult> Follow(string displayName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized(); 
            } 
            await _userService.FollowAsync(currentUser.Id, displayName); 
            return RedirectToAction("Details", new { displayName }); 
        }

        [HttpPost("Unfollow/{displayName}")]
        public async Task<IActionResult> Unfollow(string displayName) 
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            await _userService.UnfollowAsync(currentUser.Id, displayName);
            return RedirectToAction("Details", new { displayName }); 
        }
    }
}
