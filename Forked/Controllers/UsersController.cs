using Forked.Models.Domains;
using Forked.Services.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Forked.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService; 
        private readonly UserManager<User> _userManager; 
        public UsersController(IUserService userService, UserManager<User> userManager)
        {
            _userService = userService;
            _userManager = userManager; 
        }

        // GET: /User/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser.Id;
            var userDetails = await _userService.GetUserDetailsAsync(id, currentUserId);
            if (userDetails == null)
            {
                return NotFound();
            }

            return View(userDetails);
        }

        // GET: /User/Followers/{id}
        public async Task<IActionResult> Followers(string id, int page = 1, int pageSize = 12) 
        {
            var followers = await _userService.GetFollowersAsync(id, page, pageSize);
            return View(followers); 
        }

        // GET: /User/Following/{id}
        public async Task<IActionResult> Following(string id, int page = 1, int pageSize = 12) 
        {
            var following = await _userService.GetFollowingAsync(id, page, pageSize);

            return View(following);
        }

        // GET: /User/Card/{id}
        public async Task<IActionResult> Card(string id) 
        { 
            var card = await _userService.GetUserCardAsync(id);
            if (card == null)
            {
                return NotFound();
            }

            return PartialView("_UserCard", card);
        }
    }
}
