using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Reviews
{
    public class UpdateReviewViewModel
    {
        public int Id { get; set; }

        [Required]
        public int RecipeId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;

        public List<string> ExistingImagePaths { get; set; } = new();

        [FromForm]
        public List<IFormFile>? NewImages { get; set; }
    }
}
