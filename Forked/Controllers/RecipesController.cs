using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;
using Forked.Services.Ingredients;
using Forked.Services.Recipes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Forked.Controllers
{
    public class RecipesController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly UserManager<User> _userManager;
        private readonly IIngredientParser _ingredientParser;

        public RecipesController(
            IRecipeService recipeService,
            UserManager<User> userManager,
            IIngredientParser ingredientParser)
        {
            _recipeService = recipeService;
            _userManager = userManager;
            _ingredientParser = ingredientParser;
        }

        private static List<CreateRecipeViewModel> _recipes = new();

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
                // Temporarily expose what's failing
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}");
                TempData["DebugErrors"] = string.Join(" | ", errors);
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var recipe = await _recipeService.CreateAsync(vm, user.Id);

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            try
            {
                await _recipeService.DeleteRecipeAsync(id, user.Id);
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
