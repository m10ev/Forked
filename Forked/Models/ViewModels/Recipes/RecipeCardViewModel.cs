using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Recipes
{
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
                if (!TotalTimeInMinutes.HasValue || TotalTimeInMinutes <= 0)
                {
                    return "N/A";
                }

                var minutes = TotalTimeInMinutes.Value;

                if (minutes < 60)
                {
                    return $"{minutes} min";
                }
                else
                {
                    var hours = minutes / 60;
                    var remainingMinutes = minutes % 60;
                    if (remainingMinutes == 0)
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
}
