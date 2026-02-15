using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Recipes
{
    public class RecipeFilterViewModel
    {
        [Display(Name = "Search")]
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters.")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Maximum Preparation Time (minutes)")]
        [Range(1, 2880, ErrorMessage = "Preparation time must be between 1 and 1440 minutes.")]
        public int? MaxPreparationTime { get; set; }

        [Display(Name = "Maximum Cooking Time (minutes)")]
        [Range(1, 1440, ErrorMessage = "Cooking time must be between 1 and 1440 minutes.")]
        public int? MaxCookingTime { get; set; }

        [Display(Name = "Min Servings")]
        [Range(1, 100)]
        public int? MinServings { get; set; }

        [Display(Name = "Max Servings")]
        [Range(1, 100)]
        public int? MaxServings { get; set; }

        [Display(Name = "Minimum Rating")]
        [Range(0, 5)]
        public double? MinimumRating { get; set; }

        [Display(Name = "Author")]
        public string? AuthorName { get; set; }

        [Display(Name = "Only My Recipes")]
        public bool OnlyMyRecipes { get; set; } = false;
        [Display(Name = "Only My Favourites")]
        public bool OnlyFavourites { get; set; } = false;
        [Display(Name = "Only Forked Recipes")]
        public bool OnlyForked { get; set; } = false;
        [Display(Name = "Only Original Recipes")]
        public bool OnlyOriginals { get; set; } = false;

        public bool HasActiveFilters() =>
            !string.IsNullOrWhiteSpace(SearchTerm) ||
            MaxPreparationTime.HasValue ||
            MaxCookingTime.HasValue ||
            MinServings.HasValue ||
            MaxServings.HasValue ||
            MinimumRating.HasValue ||
            !string.IsNullOrWhiteSpace(AuthorName) ||
            OnlyFavourites ||
            OnlyMyRecipes ||
            OnlyForked ||
            OnlyOriginals;
    }

    public enum RecipeSortOption
    {
        Newest,
        Oldest,
        MostPopular,
        HighestRated,
        Alphabetical,
        Quickest,
        MostForked
    }
}
