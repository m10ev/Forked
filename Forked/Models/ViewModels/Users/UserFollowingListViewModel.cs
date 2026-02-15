using Forked.Models.ViewModels.Shared;

namespace Forked.Models.ViewModels.Users
{
    public class UserFollowingListViewModel : PagedResultViewModel<UserCardViewModel>
    {
        public UserFollowingListViewModel() : base() { }

        public UserFollowingListViewModel(IEnumerable<UserCardViewModel> items, int currentPage, int pageSize, int totalItems) : base(items, currentPage, pageSize, totalItems)
        {
        }
    }
}
