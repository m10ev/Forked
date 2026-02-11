using Forked.Models.Domains;
using Forked.Models.ViewModels.Users;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Forked.Extensions.Mapping
{
    public static class UserMappingExtensions
    {
        public static UserDetailsViewModel ToDetailsViewModel(this User user)
        {
            return new UserDetailsViewModel
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                ProfilePicturePath = user.ProfilePicturePath,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                ForkedCount = ,
                RecipeCount = user.Recipes.Count
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

       /* public static UserFollowersListViewModel ToFollowersListViewModel(this User user)
        {
            return UserFollowersListViewModel
            {

            }
        };*/
    }
}
