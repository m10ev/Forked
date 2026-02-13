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

        public async Task DeleteAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found!");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserFollowersListViewModel> GetFollowersAsync(string userId, int page, int pageSize)
        {
            var user = await _context.Users.Include(u => u.Followers)
                .ThenInclude(f => f.Follower)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user!.ToFollowersListModel(page, pageSize);
        }

        public async Task<UserFollowingListViewModel> GetFollowingAsync(string userId, int page, int pageSize)
        {
            var user = await _context.Users.Include(u => u.Following)
                .ThenInclude(f => f.Following)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user!.ToFollowingListModel(page, pageSize);
        }

        public async Task<UserCardViewModel?> GetUserCardAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            return user?.ToCardViewModel();
        }

        public async Task<UserDetailsViewModel?> GetUserDetailsAsync(string userId, string? currentUserId)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .Include(u => u.Recipes)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.ToDetailsViewModel();
        }
    }
}
