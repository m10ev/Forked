using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Reviews
{
    public class CreateReviewViewModel
    {
        public int RecipeId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
        public string Message { get; set; } = string.Empty;

        [FromForm]
        public List<IFormFile>? Images { get; set; }
    }

}
