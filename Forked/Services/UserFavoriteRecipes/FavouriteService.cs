using Forked.Data;
using Forked.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Forked.Services.UserFavoriteRecipes
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ForkedDbContext _context;

        public FavoriteService(ForkedDbContext context)
        {
            _context = context;
        }

        public async Task AddFavouriteAsync(string userId, int recipeId)
        {
            // Prevent duplicates
            if (!await _context.UserFavoriteRecipes.AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId))
            {
                var deletedFavorite = await _context.UserFavoriteRecipes.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);
                if (deletedFavorite != null)
                {
                    deletedFavorite.DeletedAt = null; // Restore the soft-deleted record
                    _context.UserFavoriteRecipes.Update(deletedFavorite);
                }
                else
                {
                    _context.UserFavoriteRecipes.Add(new UserFavoriteRecipe()
                    {
                        UserId = userId,
                        RecipeId = recipeId
                    });
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFavouriteAsync(string userId, int recipeId)
        {
            var favourite = await _context.UserFavoriteRecipes
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);

            if (favourite != null)
            {
                _context.UserFavoriteRecipes.Remove(favourite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsFavouriteAsync(string userId, int recipeId)
        {
            return await _context.UserFavoriteRecipes.AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId);
        }
    }

}
