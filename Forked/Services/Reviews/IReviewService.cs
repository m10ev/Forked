using Forked.Models.Domains;
using Forked.Models.ViewModels.Reviews;
using Forked.Models.ViewModels.Shared;

namespace Forked.Services.Reviews
{
    public interface IReviewService
    {
        Task<Review> AddReviewAsync(CreateReviewViewModel vm, string userId);
        Task<UpdateReviewViewModel?> GetReviewForEditAsync(int reviewId, string userId);
        Task<Review> UpdateReviewAsync(UpdateReviewViewModel vm, string userId);
        Task<PagedResultViewModel<ReviewViewModel>> GetPagedReviewsAsync(int recipeId, string? currentUserId, int page = 1, int pageSize = 5);
        Task DeleteReviewAsync(int reviewId, string userId, string role);
    }
}
