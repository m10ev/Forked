using Forked.Models.ViewModels.Shared;

namespace Forked.Models.ViewModels.Users
{
    public class UserFollowersListViewModel : PagedResultViewModel<UserCardViewModel>
    {
        public UserFollowersListViewModel() : base() { }

        public UserFollowersListViewModel(IEnumerable<UserCardViewModel> items, int currentPage, int pageSize, int totalItems) : base(items, currentPage, pageSize, totalItems)
        {
        }
    }
}
