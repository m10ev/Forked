using Forked.Models.Domains;
using Forked.Models.ViewModels.Users;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Forked.Extensions.Mapping
{
    public static class UserMappingExtensions
    {
        public static UserDetailsViewModel ToDetailsViewModel(this User user, string? currentUserId)
        {
            return new UserDetailsViewModel
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                ProfilePicturePath = user.ProfilePicturePath,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                RecipeCount = user.Recipes.Count,
                IsCurrentUser = user.Id == currentUserId, 
                IsFollowedByCurrentUser = user.Followers .Any(f => f.FollowerId == currentUserId)
            };
        }

        public static UserCardViewModel ToCardViewModel(this User user)
        {
            return new UserCardViewModel
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Description = user.Bio,
                ProfilePicturePath = user.ProfilePicturePath
            };
        }

        public static UserFollowersListViewModel ToFollowersListModel(this User user, int currentPage, int pageSize)
        {
            var followers = user.Followers.Select(f => f.Following).Select(f => f.ToCardViewModel()).ToList();
            return new UserFollowersListViewModel(followers, currentPage, pageSize, followers.Count);
        }

        public static UserFollowingListViewModel ToFollowingListModel (this User user, int currentPage, int pageSize)
        {
            var following = user.Following.Select(f => f.Following).Select(f => f.ToCardViewModel()).ToList();
            return new UserFollowingListViewModel(following, currentPage, pageSize, following.Count);
        }


    }
}
