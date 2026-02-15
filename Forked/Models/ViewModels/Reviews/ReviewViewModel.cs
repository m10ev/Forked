namespace Forked.Models.ViewModels.Reviews
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public int Rating { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> ImagePaths { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
