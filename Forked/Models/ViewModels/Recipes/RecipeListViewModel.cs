using Forked.Models.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Recipes
{
    public class RecipeListViewModel : PagedResultViewModel<RecipeCardViewModel>
    {
        public RecipeFilterViewModel Filters { get; set; } = new RecipeFilterViewModel();
        public RecipeSortOption SortBy { get; set; } = RecipeSortOption.MostPopular;

        public RecipeListViewModel() : base()
        {
        }

        public RecipeListViewModel(IEnumerable<RecipeCardViewModel> recipes, int currentPage, int pageSize, int totalItems) : base(recipes, currentPage, pageSize, totalItems)
        {
        }
    }

    public class RecipeCardViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Recipe Title")]
        public string Title { get; set; } = string.Empty;
        [Display(Name = "Description")]
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        [Display(Name = "Total time")]
        public int? TotalTimeInMinutes { get; set; }
        public int Servings { get; set; }
        [Display(Name = "Author")]
        public string AuthorName { get; set; } = string.Empty;
        [Display(Name = "Rating")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AverageRating { get; set; }
        [Display(Name = "Reviews")]
        public int ReviewCount { get; set; }
        public bool IsForked { get; set; }
        public bool IsFavoritedByCurrentUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeDisplay
        {
            get
            {
                if(!TotalTimeInMinutes.HasValue || TotalTimeInMinutes <= 0)
                {
                    return "N/A";
                }

                var minutes = TotalTimeInMinutes.Value;

                if(minutes < 60)
                {
                    return $"{minutes} min";
                }
                else
                {
                    var hours = minutes / 60;
                    var remainingMinutes = minutes % 60;
                    if(remainingMinutes == 0)
                    {
                        return $"{hours} hr{(hours > 1 ? "s" : "")}";
                    }
                    else
                    {
                        return $"{hours} hr{(hours > 1 ? "s" : "")} {remainingMinutes} min";
                    }
                }
            }
        }
    }

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
        public string? AuthorId { get; set; }
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
            !string.IsNullOrWhiteSpace(AuthorId) ||
            OnlyFavourites ||
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
