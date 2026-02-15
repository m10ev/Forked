using Forked.Models.Interfaces;

namespace Forked.Models.Domains
{
    public class UserFollow : IAuditable
    {
        public string FollowerId { get; set; } = null!;
        public User Follower { get; set; } = null!;
        public string FollowingId { get; set; } = null!;
        public User Following { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
