using Forked.Data;
using Forked.Extensions.Mapping;
using Forked.Models.Domains;
using Forked.Models.ViewModels.Reviews;
using Forked.Models.ViewModels.Shared;
using Forked.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Forked.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly ForkedDbContext _context;
        private readonly IImageService _imageService;

        public ReviewService(ForkedDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<Review> AddReviewAsync(CreateReviewViewModel vm, string userId)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            if (vm.Rating < 1 || vm.Rating > 5)
                throw new ArgumentOutOfRangeException(nameof(vm.Rating), "Rating must be between 1 and 5.");

            if (string.IsNullOrWhiteSpace(vm.Message))
                throw new ArgumentException("Message cannot be empty.", nameof(vm.Message));

            var review = await vm.ToReview(userId, _imageService);

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return review;
        }

        public async Task<UpdateReviewViewModel?> GetReviewForEditAsync(int reviewId, string userId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return null;

            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own reviews.");

            return review.ToEditViewModel();
        }

        public async Task<Review> UpdateReviewAsync(UpdateReviewViewModel vm, string userId)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == vm.Id);

            if (review == null)
                throw new KeyNotFoundException($"Review with ID {vm.Id} not found.");

            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own reviews.");

            await review.UpdateFromViewModelAsync(vm, _imageService);

            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<PagedResultViewModel<ReviewViewModel>> GetPagedReviewsAsync(
            int recipeId,
            string? currentUserId,
            int page = 1,
            int pageSize = 5)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Reviews
                .AsNoTracking()
                .Where(r => r.RecipeId == recipeId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt);

            var totalReviews = await query.CountAsync();

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reviewViewModels = reviews.Select(r => r.ToViewModel(currentUserId)).ToList();

            return new PagedResultViewModel<ReviewViewModel>(
                reviewViewModels,
                page,
                pageSize,
                totalReviews
            );
        }

        public async Task DeleteReviewAsync(int reviewId, string userId, string role)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
                throw new KeyNotFoundException($"Review with ID {reviewId} not found.");

            if (review.UserId != userId && role != "Admin")
                throw new UnauthorizedAccessException("You do not have permission to delete this review.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}