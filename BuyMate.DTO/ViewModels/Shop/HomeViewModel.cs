using BuyMate.DTO.ViewModels.Category;
using BuyMate.DTO.ViewModels.Product;

namespace BuyMate.DTO.ViewModels.Shop
{
    public class HomeViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<ProductViewModel> FeaturedProducts { get; set; } = new();
        public List<ProductViewModel> BestSellers { get; set; } = new();
    }

}
