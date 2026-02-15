using Forked.Models.Interfaces;

namespace Forked.Models.Domains
{
    public class Review : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        public int Rating { get; set; }
		public string Message { get; set; } = string.Empty;
        public List<string> ImagePaths { get; set; } = new List<string>();
    }
}
