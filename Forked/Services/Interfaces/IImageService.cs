namespace Forked.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveRecipeImageAsync(IFormFile file);
        Task<string> SaveStepImageAsync(IFormFile file);
        Task<string> SaveReviewImageAsync(IFormFile file);
        Task DeleteAsync(string path);
    }

}
