using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;
using Forked.Services.Ingredients;
using Forked.Services.Recipes;
using Forked.Services.UserFavoriteRecipes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Forked.Controllers
{
    public class RecipesController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly IFavoriteService _favoriteService;
        private readonly UserManager<User> _userManager;
        private readonly IIngredientParser _ingredientParser;

        public RecipesController(
            IRecipeService recipeService,
            IFavoriteService favoriteService,
            UserManager<User> userManager,
            IIngredientParser ingredientParser)
        {
            _recipeService = recipeService;
            _favoriteService = favoriteService;
            _userManager = userManager;
            _ingredientParser = ingredientParser;
        }

        // GET: /Recipes
        public async Task<IActionResult> Index(RecipeFilterViewModel filters, RecipeSortOption sortBy = RecipeSortOption.MostPopular, int page = 1, int pageSize = 12)
        {
            var user = await _userManager.GetUserAsync(User);
            var currentUserId = user?.Id;

            var result = await _recipeService.GetPagedRecipesAsync(
                filters,
                sortBy,
                page,
                pageSize,
                currentUserId);

            return View(result);
        }


        // GET: /Recipes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var currentUserId = user?.Id;

            var recipe = await _recipeService.GetRecipeDetailAsync(id, currentUserId);

            if (recipe == null)
                return NotFound();

            return View(recipe);
        }


        // GET: /Recipes/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateRecipeViewModel());
        }


        // POST: /Recipes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRecipeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var recipe = await _recipeService.CreateAsync(vm, user.Id);

            return RedirectToAction("Details", new { id = recipe.Id });
        }

        [HttpGet("Recipes/Fork/{id}")]
        public async Task<IActionResult> Fork(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var forkVm = await _recipeService.PrepareForkAsync(id);

            if (forkVm == null) return NotFound();
            return View(forkVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fork(CreateForkViewModel vm)
        {
            if(!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var recipe = await _recipeService.ForkAsync(vm, user.Id);

            return RedirectToAction("Details", new { id = recipe.Id });
        }

        // AJAX: Parse single ingredient
        [HttpPost]
        public IActionResult ParseIngredient([FromBody] string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return BadRequest("Empty ingredient");

            var parsed = _ingredientParser.Parse(input);

            if (string.IsNullOrWhiteSpace(parsed.Name))
                return BadRequest("Could not parse ingredient");

            return Json(new
            {
                quantity = parsed.Quantity,
                unit = parsed.Unit ?? "",
                name = parsed.Name,
                preparation = parsed.Preparation ?? "",
                formatted = _ingredientParser.Format(parsed)
            });
        }

        [HttpPost]
        public async Task<IActionResult> FavoriteRecipe(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _favoriteService.AddFavouriteAsync(user.Id, id);
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> UnfavoriteRecipe(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _favoriteService.RemoveFavouriteAsync(user.Id, id);
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            bool isAdmin = roles.Contains("Admin");

            try
            {
                await _recipeService.DeleteRecipeAsync(id, user.Id, isAdmin);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
