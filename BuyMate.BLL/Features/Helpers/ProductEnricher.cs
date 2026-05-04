using BuyMate.BLL.Contracts;
using BuyMate.DTO.ViewModels.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.BLL.Features.Helpers
{
    public class ProductEnricher : IProductEnricher
    {
        private readonly IWishlistService _wishlistService;

        public ProductEnricher(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        public async Task EnrichAsync(IEnumerable<ProductViewModel> products, ClaimsPrincipal user)
        {
            if (products == null || !products.Any())
                return;

            if (user.Identity?.IsAuthenticated != true)
                return;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return;

            var wishlistResponse = await _wishlistService.GetWishlistAsync(userId);
            var wishlist = wishlistResponse.Data;

            if (wishlist?.Items == null || !wishlist.Items.Any())
                return;

            var wishlistIds = wishlist.Items
                                      .Select(x => x.ProductId)
                                      .ToHashSet();

            foreach (var product in products)
            {
                product.IsInWishlist = wishlistIds.Contains(product.Id);
            }
        }
    }
}
