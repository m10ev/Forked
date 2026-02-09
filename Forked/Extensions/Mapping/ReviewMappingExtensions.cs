using Forked.Models.ViewModels.Reviews;

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
    }
}
