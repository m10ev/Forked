using Forked.Models.ViewModels.Shared;

namespace Forked.Models.ViewModels.Users
{
    public class UserFollowingListViewModel : PagedResultViewModel<UserFollowerCardViewModel>
    {
        public UserFollowingListViewModel() :base() { }

        public UserFollowingListViewModel(IEnumerable<UserFollowerCardViewModel> items, int currentPage, int pageSize, int totalItems) : base(items, currentPage, pageSize, totalItems)
        {
        }
    }
}
