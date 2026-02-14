using Forked.Models.Domains;
using Forked.Models.ViewModels.Users;

namespace Forked.Services.Users
{
    public interface IUserService
    {
        Task<User> CreateAsync(User user);
        Task<UserDetailsViewModel?> GetUserDetailsAsync(string userId, string? currentUserId);
        Task<UserFollowersListViewModel> GetFollowersAsync(string userId, int page, int pageSize); 
        Task<UserFollowingListViewModel> GetFollowingAsync(string userId, int page, int pageSize);
        Task<UserCardViewModel?> GetUserCardAsync(string userId);
        Task DeleteAsync(string userId);
        Task FollowAsync(string id, string displayName);
        Task UnfollowAsync(string id, string displayName);
    }
}

