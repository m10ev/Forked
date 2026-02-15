using Forked.Models.Domains;
using Forked.Models.ViewModels.Reviews;
using Forked.Services.Interfaces;
using Forked.Services.Reviews;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace Forked.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly UserManager<User> _userManager;

        public ReviewsController(IReviewService reviewService, UserManager<User> userManager)
        {
            _reviewService = reviewService;
            _userManager = userManager;
        }

        // AJAX: GET /Reviews/GetPaged?recipeId=1&page=1&pageSize=5
        [HttpGet]
        public async Task<IActionResult> GetPaged(int recipeId, int page = 1, int pageSize = 5)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _reviewService.GetPagedReviewsAsync(recipeId, user?.Id, page, pageSize);
            return Json(result);
        }

        // POST: /Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel vm)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", "Recipes", new { id = vm.RecipeId });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _reviewService.AddReviewAsync(vm, user.Id);
            }
            catch (Exception)
            {
                
            }

            return RedirectToAction("Details", "Recipes", new { id = vm.RecipeId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var review = await _reviewService.GetReviewForEditAsync(id, user.Id);
            if (review == null) return NotFound();

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateReviewViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _reviewService.UpdateReviewAsync(vm, user.Id);
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return Forbid(); }

            return RedirectToAction("Details", "Recipes", new { id = vm.RecipeId });
        }

        // POST: /Reviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int recipeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Contains("Admin") ? "Admin" : "User";

            try
            {
                await _reviewService.DeleteReviewAsync(id, user.Id, role);
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return Forbid(); }

            return RedirectToAction("Details", "Recipes", new { id = recipeId });
        }
    }
}