using Forked.Models.Domains;
using Forked.Models.ViewModels.Reviews;
using Forked.Services;
using Forked.Services.Interfaces;
using System.Threading.Tasks;

namespace Forked.Extensions.Mapping
{
    public static class ReviewMappingExtensions
    {
        public static ReviewViewModel ToViewModel(this Models.Domains.Review review, string? currentUserId = null)
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
                IsCurrentUser =  !string.IsNullOrEmpty(currentUserId) && review.UserId == currentUserId
            };
        }

        public static async Task<Review> ToReview(this CreateReviewViewModel viewModel, string userId, IImageService imageService)
        {
            var review = new Review
            {
                UserId = userId,
                Rating = viewModel.Rating,
                Message = viewModel.Message,
                ImagePaths = new List<string>()
            };

            // Handle image uploads and populate ImagePaths
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
    }
}
