using Forked.Models.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace Forked.Models.ViewModels.Recipes
{
    public class RecipeListViewModel : PagedResultViewModel<RecipeCardViewModel>
    {
        public RecipeFilterViewModel Filters { get; set; } = new RecipeFilterViewModel();
        public RecipeSortOption SortBy { get; set; } = RecipeSortOption.MostPopular;

        public RecipeListViewModel() : base()
        {
        }

        public RecipeListViewModel(IEnumerable<RecipeCardViewModel> recipes, int currentPage, int pageSize, int totalItems) : base(recipes, currentPage, pageSize, totalItems)
        {
        }
    }
}
