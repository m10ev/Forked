using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;
using Forked.Services.Recipes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Forked.Controllers
{
    public class RecipesController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly UserManager<User> _userManager;

        public RecipesController(
            IRecipeService recipeService,
            UserManager<User> userManager)
        {
            _recipeService = recipeService;
            _userManager = userManager;
        }

        private static List<CreateRecipeViewModel> _recipes = new();

        // GET: /Recipes
        public IActionResult Index()
        {
            return View(_recipes);
        }

        // GET: /Recipes/Create
        public IActionResult Create()
        {
            var vm = new CreateRecipeViewModel();
            vm.Ingredients.Add(new CreateRecipeIngredientViewModel());
            vm.Steps.Add(new CreateRecipeStepViewModel());
            return View(vm);
        }

        // POST: /Recipes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRecipeViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var authorId = user.Id;

            var recipe = await _recipeService.CreateAsync(vm, authorId);

            // For testing, you can also store it in-memory
            _recipes.Add(vm);

            return RedirectToAction("Details", new { id = recipe.Id });
        }
    }
}
