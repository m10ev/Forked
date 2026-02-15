using Forked.Data;
using Forked.Extensions.Mapping;
using Forked.Models.Domains;
using Forked.Models.ViewModels.Users;
using Microsoft.EntityFrameworkCore;

namespace Forked.Services.Users
{
    public class UserService : IUserService
    {
        private readonly ForkedDbContext _context;
        public UserService (ForkedDbContext context)
        {
            _context = context; 
        }

        public async Task<User> CreateAsync(User user)
        {
           _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteAsync(string displayName)
        {
            var user = await _context.Users.FindAsync(displayName);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found!");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserFollowersListViewModel> GetFollowersAsync(string displayName, int page, int pageSize)
        {
            var user = await _context.Users.Include(u => u.Followers)
                .ThenInclude(f => f.Follower)
                .FirstOrDefaultAsync(u => u.DisplayName == displayName);

            return user!.ToFollowersListModel(page, pageSize);
        }

        public async Task<UserFollowingListViewModel> GetFollowingAsync(string displayName, int page, int pageSize)
        {
            var user = await _context.Users.Include(u => u.Following)
                .ThenInclude(f => f.Following)
                .FirstOrDefaultAsync(u => u.DisplayName == displayName);

            return user!.ToFollowingListModel(page, pageSize);
        }

        public async Task<UserCardViewModel?> GetUserCardAsync(string displayName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.DisplayName == displayName);

            return user?.ToCardViewModel();
        }

        public async Task<UserDetailsViewModel?> GetUserDetailsAsync(string displayName, string? currentUserId)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .Include(u => u.Recipes)
                .FirstOrDefaultAsync(u => u.DisplayName == displayName);

            return user?.ToDetailsViewModel(currentUserId);
        }

        public async Task FollowAsync(string currentUserId, string targetDisplayName)
        {
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.DisplayName == targetDisplayName);
            if (targetUser == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            bool alreadyFollowing = await _context.UserFollows.AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUser.Id); 
            if (!alreadyFollowing) 
            {
                var deletedFollowing = await _context.UserFollows.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUser.Id);
                if (deletedFollowing != null)
                {
                    deletedFollowing.DeletedAt = null;
                }
                else
                {
                    _context.UserFollows.Add(new UserFollow { FollowerId = currentUserId, FollowingId = targetUser.Id });
                }

                await _context.SaveChangesAsync(); 
            } 
        }

        public async Task UnfollowAsync(string currentUserId, string targetDisplayName) 
        {
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.DisplayName == targetDisplayName);
            if (targetUser == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            var follow = await _context.UserFollows.FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUser.Id);
            if (follow != null) 
            {
                _context.UserFollows.Remove(follow); 
                await _context.SaveChangesAsync(); 
            }
        }
    }
}
