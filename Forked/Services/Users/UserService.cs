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

            return user?.ToDetailsViewModel();
        }
    }
}
