using Forked.Models.Domains;
using Forked.Models.ViewModels.Users;

namespace Forked.Services.Users
{
    public interface IUserService
    {
        Task<User> CreateAsync(User user);
        Task<UserDetailsViewModel?> GetUserDetailsAsync(string displayName, string? currentUserId);
        Task<UserFollowersListViewModel> GetFollowersAsync(string displayName, int page, int pageSize); 
        Task<UserFollowingListViewModel> GetFollowingAsync(string displayName, int page, int pageSize);
        Task<UserCardViewModel?> GetUserCardAsync(string displayName);
        Task DeleteAsync(string displayName);
        Task FollowAsync(string id, string displayName);
        Task UnfollowAsync(string id, string displayName);
    }
}

