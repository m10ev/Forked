using Forked.Services.Interfaces;

namespace Forked.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;

        private const string ImageRoot = "images";
        private const string RecipeFolder = "images/recipes";
        private const string StepFolder = "images/steps";

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public Task<string> SaveRecipeImageAsync(IFormFile file)
       => SaveAsync(file, RecipeFolder);

        public Task<string> SaveStepImageAsync(IFormFile file)
            => SaveAsync(file, StepFolder);

        private async Task<string> SaveAsync(IFormFile file, string relativeFolder)
        {
            Validate(file);

            var physicalFolder = Path.Combine(_env.WebRootPath, relativeFolder);
            Directory.CreateDirectory(physicalFolder);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var physicalPath = Path.Combine(physicalFolder, fileName);

            using var stream = new FileStream(physicalPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // This is what you store in the DB
            return $"/{relativeFolder}/{fileName}";
        }

        public Task DeleteAsync(string path)
        {
            var physicalPath = Path.Combine(
                _env.WebRootPath,
                path.TrimStart('/'));

            if (File.Exists(physicalPath))
                File.Delete(physicalPath);

            return Task.CompletedTask;
        }

        private static void Validate(IFormFile file)
        {
            if (file.Length == 0)
                throw new InvalidOperationException("Empty file");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("File too large");

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowed.Contains(ext))
                throw new InvalidOperationException("Invalid file type");
        }
    }
}
