using Forked.Models.ViewModels.Reviews;

namespace Forked.Models.ViewModels.Recipes
{
    public class RecipeDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<string> ImagePaths { get; set; } = new();
        public int? PreparationTimeInMinutes { get; set; }
        public int? CookingTimeInMinutes { get; set; }
        public int TotalTimeInMinutes => (PreparationTimeInMinutes ?? 0) + (CookingTimeInMinutes ?? 0);
        public int Servings { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Author Information
        public string AuthorId { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public bool IsAuthor { get; set; }

        // Fork Information
        public int? ParentRecipeId { get; set; }
        public string? ParentRecipeTitle { get; set; }
        public string? ParentRecipeAuthorName { get; set; }
        public bool IsForked => ParentRecipeId.HasValue;
        public bool IsOriginal => !IsForked;
        public int ForkCount { get; set; }

        // User Interaction
        public bool IsFavourite { get; set; }
        public bool HasUserReviewed { get; set; }
        public int? UserReviewId { get; set; }

        // Recipe Content
        public List<RecipeIngredientViewModel> Ingredients { get; set; } = new();
        public List<RecipeStepViewModel> Steps { get; set; } = new();
        public List<ReviewViewModel> Reviews { get; set; } = new();
    }
}