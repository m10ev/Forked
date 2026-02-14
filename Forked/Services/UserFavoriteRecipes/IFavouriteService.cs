namespace Forked.Services.UserFavoriteRecipes
{
    public interface IFavoriteService
    {
        Task AddFavouriteAsync(string userId, int recipeId);
        Task RemoveFavouriteAsync(string userId, int recipeId);
        Task<bool> IsFavouriteAsync(string userId, int recipeId);
    }

}
