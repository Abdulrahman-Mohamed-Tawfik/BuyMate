using BuyMate.DTO.ViewModels.Product;

namespace BuyMate.DTO.ViewModels.Shop
{
    public class SearchViewModel
    {
        public string? Query { get; set; }
        public List<ProductViewModel> Results { get; set; } = new List<ProductViewModel>();
    }
}
