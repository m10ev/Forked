using Forked.Models.Domains;
using Forked.Models.ViewModels.Reviews;
using Forked.Services.Interfaces;

namespace Forked.Extensions.Mapping
{
    public static class ReviewMappingExtensions
    {
        public static ReviewViewModel ToViewModel(this Review review, string? currentUserId = null)
        {
            return new ReviewViewModel
            {
                Id = review.Id,
                UserId = review.UserId,
                DisplayName = review.User?.DisplayName ?? "Unknown",
                Rating = review.Rating,
                Message = review.Message,
                ImagePaths = review.ImagePaths,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                IsCurrentUser = !string.IsNullOrEmpty(currentUserId) && review.UserId == currentUserId
            };
        }

        public static async Task<Review> ToReview(this CreateReviewViewModel viewModel, string userId, IImageService imageService)
        {
            var review = new Review
            {
                RecipeId = viewModel.RecipeId,
                UserId = userId,
                Rating = viewModel.Rating,
                Message = viewModel.Message,
                ImagePaths = new List<string>()
            };

            if (viewModel.Images != null)
            {
                foreach (var image in viewModel.Images)
                {
                    var path = await imageService.SaveReviewImageAsync(image);
                    review.ImagePaths.Add(path);
                }
            }

            return review;
        }

        // Maps Review → UpdateReviewViewModel for the edit form
        public static UpdateReviewViewModel ToEditViewModel(this Review review)
        {
            return new UpdateReviewViewModel
            {
                Id = review.Id,
                RecipeId = review.RecipeId,
                Rating = review.Rating,
                Message = review.Message,
                ExistingImagePaths = review.ImagePaths.ToList()
            };
        }

        // Applies UpdateReviewViewModel onto an existing Review, handling image diff
        public static async Task UpdateFromViewModelAsync(
            this Review review,
            UpdateReviewViewModel vm,
            IImageService imageService)
        {
            review.Rating = vm.Rating;
            review.Message = vm.Message;

            // Image diff — delete removed images
            var existingPaths = vm.ExistingImagePaths ?? new List<string>();
            var removedImages = review.ImagePaths
                .Where(img => !existingPaths.Contains(img))
                .ToList();

            foreach (var img in removedImages)
                await imageService.DeleteAsync(img);

            review.ImagePaths = existingPaths.ToList();

            // Add new images
            if (vm.NewImages?.Any() == true)
            {
                foreach (var image in vm.NewImages)
                {
                    var path = await imageService.SaveReviewImageAsync(image);
                    review.ImagePaths.Add(path);
                }
            }
        }
    }
}