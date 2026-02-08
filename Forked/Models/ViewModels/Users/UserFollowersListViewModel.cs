using Forked.Models.ViewModels.Shared;

namespace Forked.Models.ViewModels.Users
{
    public class UserFollowersListViewModel : PagedResultViewModel<UserFollowerCardViewModel>
    {
        public UserFollowersListViewModel() : base() { }

        public UserFollowersListViewModel(IEnumerable<UserFollowerCardViewModel> items, int currentPage, int pageSize, int totalItems) : base(items, currentPage, pageSize, totalItems)
        {
        }
    }
}
