using Forked.Data;
using Forked.Extensions.Mapping;
using Forked.Models.Domains;
using Forked.Models.ViewModels.Recipes;
using Forked.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Forked.Services.Recipes
{
    public class RecipeService : IRecipeService
    {
        private readonly ForkedDbContext _context;
        private readonly IImageService _imageService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RecipeService(ForkedDbContext context, IImageService imageService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _imageService = imageService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<Recipe> CreateAsync(
            CreateRecipeViewModel vm,
            string authorId,
            int? parentRecipeId = null)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            if (vm.ParsedIngredients == null || !vm.ParsedIngredients.Any())
                throw new ArgumentException("Recipe must contain at least one ingredient.");

            // Create the base recipe entity
            var recipe = await vm.ToRecipe(authorId, _imageService);

            if (parentRecipeId.HasValue)
                recipe.ParentRecipeId = parentRecipeId.Value;

            // Map recipe steps
            recipe.RecipeSteps = await vm.Steps.ToRecipeStepsAsync(_imageService);

            // Map parsed ingredients to domain
            recipe.RecipeIngredients = await vm.ParsedIngredients.ToRecipeIngredientsAsync(_context);

            // Add recipe to context and save
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return recipe;
        }

        public async Task<CreateForkViewModel> PrepareForkAsync(int originalRecipeId)
        {
            var original = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeSteps)
                .FirstOrDefaultAsync(r => r.Id == originalRecipeId);

            if (original == null) throw new KeyNotFoundException("Original recipe not found");

            return await original.PrepareForkAsync();
        }

        public async Task<Recipe> ForkAsync(CreateForkViewModel vm, string authorId)
        {
            if(vm == null)
                throw new ArgumentNullException(nameof(vm));

            if(vm.ParsedIngredients == null || !vm.ParsedIngredients.Any())
                throw new ArgumentException("Recipe must contain at least one ingredient.");

            // Create the base recipe entity
            var recipe = await vm.ToRecipeAsync(authorId, _imageService);

            //
            recipe.RecipeSteps = await vm.Steps.ToRecipeStepsAsync(_imageService);

            // Map recipe steps
            recipe.RecipeIngredients = await vm.ParsedIngredients.ToRecipeIngredientsAsync(_context);

            // Add recipe to context and save
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return recipe;
        }


        public async Task<RecipeDetailViewModel?> GetRecipeDetailAsync(int id, string? currentUserId)
        {
            var recipe = await _context.Recipes
                .AsNoTracking()
                .Include(r => r.Author)
                .Include(r => r.ParentRecipe)
                    .ThenInclude(p => p.Author)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeSteps)
                .Include(r => r.Reviews)
                    .ThenInclude(rev => rev.User)
                .Include(r => r.Forks)
                .Include(r => r.FavoritedByUsers)
                .FirstOrDefaultAsync(r => r.Id == id);

            return recipe?.ToDetailViewModel(currentUserId);
        }

        public async Task<RecipeListViewModel> GetPagedRecipesAsync(RecipeFilterViewModel filters, RecipeSortOption sortBy, int page, int pageSize, string? currentUserId)
        {
            var query = _context.Recipes
                .AsNoTracking()
                .AsQueryable();

            // --- Filters (same as before) ---
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var search = filters.SearchTerm.Trim().ToLower();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(search) ||
                    r.Description.ToLower().Contains(search));
            }

            if (filters.MaxPreparationTime.HasValue)
                query = query.Where(r =>
                    r.PreparationTimeInMinutes <= filters.MaxPreparationTime);

            if (filters.MaxCookingTime.HasValue)
                query = query.Where(r =>
                    r.CookingTimeInMinutes <= filters.MaxCookingTime);

            if (filters.MinServings.HasValue)
                query = query.Where(r =>
                    r.Servings >= filters.MinServings);

            if (filters.MaxServings.HasValue)
                query = query.Where(r =>
                    r.Servings <= filters.MaxServings);

            if (!string.IsNullOrEmpty(filters.AuthorName))
            {
                query = query.Include(r => r.Author).Where(r => r.Author.DisplayName == filters.AuthorName);
            }

            if (filters.OnlyForked)
                query = query.Where(r => r.ParentRecipeId != null);

            if (filters.OnlyOriginals)
                query = query.Where(r => r.ParentRecipeId == null);

            if (filters.OnlyFavourites && !string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(r =>
                    r.FavoritedByUsers.Any(f => f.UserId == currentUserId));
            }

            if (filters.OnlyMyRecipes && !string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(r => r.AuthorId == currentUserId);
            }

            if (filters.MinimumRating.HasValue)
            {
                query = query.Where(r =>
                    r.Reviews.Any() &&
                    r.Reviews.Average(x => x.Rating) >= filters.MinimumRating);
            }

            // --- Sorting ---
            query = sortBy switch
            {
                RecipeSortOption.Newest => query.OrderByDescending(r => r.CreatedAt),
                RecipeSortOption.Oldest => query.OrderBy(r => r.CreatedAt),
                RecipeSortOption.HighestRated => query.OrderByDescending(r =>
                    r.Reviews.Any() ? r.Reviews.Average(x => x.Rating) : 0),
                RecipeSortOption.Alphabetical => query.OrderBy(r => r.Title),
                RecipeSortOption.Quickest => query.OrderBy(r =>
                    (r.PreparationTimeInMinutes ?? 0) + (r.CookingTimeInMinutes ?? 0)),
                RecipeSortOption.MostForked => query.OrderByDescending(r => r.Forks.Count),
                _ => query.OrderByDescending(r => r.Reviews.Count)
            };

            // --- Total Count ---
            var totalItems = await query.CountAsync();

            // --- Paging and mapping using mapper ---
            var recipes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => r) // keep it as Recipe for mapping
                .Include(r => r.Author)
                .Include(r => r.Reviews)
                .Include(r => r.FavoritedByUsers)
                .Include(r => r.Forks)
                .ToListAsync();

            var mapped = recipes
                .Select(r => r.ToCardViewModel(currentUserId))
                .ToList();

            return new RecipeListViewModel(mapped, page, pageSize, totalItems)
            {
                Filters = filters,
                SortBy = sortBy
            };
        }

        public async Task DeleteRecipeAsync(int id, string userId, bool isAdmin)
        {
            var recipe = await _context.Recipes.FindAsync(id);

            if (recipe == null)
                throw new KeyNotFoundException("Recipe not found");

            // Allow deletion if the user is the author or an admin
            if (recipe.AuthorId != userId && !isAdmin)
                throw new UnauthorizedAccessException("You can only delete your own recipes");

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }
    }
}
