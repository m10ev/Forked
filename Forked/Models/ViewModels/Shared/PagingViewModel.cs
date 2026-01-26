namespace Forked.Models.ViewModels.Shared
{
    public class PagingViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; } 

        bool HasPreviousPage => CurrentPage > 1;
        bool HasNextPage => CurrentPage < TotalPages;

        public int StartItem => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);

        public PagingViewModel()
        {
            CurrentPage = 1;
            PageSize = 24;
        }

        public PagingViewModel(int currentPage, int pageSize, int totalItems)
        {
            CurrentPage = currentPage < 1 ? 1 : currentPage;
            PageSize = pageSize < 1 ? 24 : pageSize;
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

            if(CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
            }
        }

        public IEnumerable<int> GetPageRange(int maxPagesToShow = 5)
        {
            if(TotalPages <= maxPagesToShow)
            {
                return Enumerable.Range(1, TotalPages);
            }

            var half = maxPagesToShow / 2;
            int start = Math.Max(1, CurrentPage - half);
            int end = Math.Min(TotalPages, start + maxPagesToShow - 1);

            if(end - start < maxPagesToShow - 1)
            {
                start = Math.Max(1, end - maxPagesToShow + 1);
            }

            return Enumerable.Range(start, end - start + 1);
        }
    }
}
