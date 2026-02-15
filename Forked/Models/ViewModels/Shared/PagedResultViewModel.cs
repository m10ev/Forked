namespace Forked.Models.ViewModels.Shared
{
    public class PagedResultViewModel<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public PagingViewModel Paging { get; set; } = new PagingViewModel();
        public PagedResultViewModel()
        {
        }
        public PagedResultViewModel(IEnumerable<T> items, int currentPage, int pageSize, int totalItems)
        {
            Items = items;
            Paging = new PagingViewModel(currentPage, pageSize, totalItems);
        }
    }
}
