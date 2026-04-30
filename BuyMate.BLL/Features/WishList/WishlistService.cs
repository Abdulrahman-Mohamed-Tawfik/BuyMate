using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Repositories;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.Wishlist;
using BuyMate.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BuyMate.BLL.Features.WishList
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;

        public WishlistService(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }


        public async Task<Response<bool>> AddtoWishlistAsync(string userId, Guid productId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                return Response<bool>.Fail("Invalid user id.");

            var wishlist = await _wishlistRepository.QueryWithDetails()
                .FirstOrDefaultAsync(w => w.UserId == userGuid);

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    UserId = userGuid,
                    Items = new List<WishlistItem>
                    {
                        new WishlistItem { ProductId = productId }
                    }
                };
                await _wishlistRepository.CreateAsync(wishlist);
                return Response<bool>.Success(true, "Product added to wishlist.");
            }

            if (wishlist.Items.Any(i => i.ProductId == productId))
            {
                return Response<bool>.Fail("Product is already in the wishlist.");
            }

            wishlist.Items.Add(new WishlistItem { ProductId = productId });
            await _wishlistRepository.UpdateAsync(wishlist);

            return Response<bool>.Success(true, "Product added to wishlist.");
        }

        public async Task<Response<bool>> ClearWishlistAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                return Response<bool>.Fail("Invalid user id.");

            var wishlist = await _wishlistRepository.QueryWithDetails()
                .FirstOrDefaultAsync(w => w.UserId == userGuid);

            if (wishlist == null)
                return Response<bool>.Fail("Wishlist not found.");

            wishlist.Items.Clear();
            await _wishlistRepository.UpdateAsync(wishlist);
            return Response<bool>.Success(true, "Wishlist cleared.");
        }

        public async Task<Response<WishlistViewModel>> GetWishlistAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                return Response<WishlistViewModel>.Fail("Invalid user id.");

            var wishlist = await _wishlistRepository.QueryWithDetails()
                .Where(w => w.UserId == userGuid)
                .Select(w => new WishlistViewModel
                {
                    Id = w.Id,
                    Items = w.Items.Select(i => new WishlistItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        Price = i.Product.Price,
                        Brand = i.Product.Brand,
                        Rating = i.Product.Reviews.Any() ? (int)i.Product.Reviews.Average(r => r.Rating) : 0,
                        ReviewCount = i.Product.Reviews.Count(),
                        ImageUrl = i.Product.Images
                            .Where(img => img.IsMain)
                            .Select(img => img.ImageUrl)
                            .FirstOrDefault()
                    }).ToList()
                })
                .SingleOrDefaultAsync();

            if (wishlist is null)
            {
                // Return an empty wishlist instead of failing, improving user experience on fresh accounts
                return Response<WishlistViewModel>.Success(new WishlistViewModel { Items = new List<WishlistItemViewModel>() });
            }

            return Response<WishlistViewModel>.Success(wishlist);
        }

        public async Task<Response<bool>> RemoveFromWishlistAsync(string userId, Guid itemId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                return Response<bool>.Fail("Invalid user id.");

            var wishlist = await _wishlistRepository.QueryWithDetails()
                .FirstOrDefaultAsync(w => w.UserId == userGuid);

            if (wishlist == null)
                return Response<bool>.Fail("Wishlist not found.");

            var item = wishlist.Items.FirstOrDefault(i => i.ProductId == itemId);
            if (item == null)
                return Response<bool>.Fail("Item not found in wishlist.");

            wishlist.Items.Remove(item);
            await _wishlistRepository.UpdateAsync(wishlist);
            return Response<bool>.Success(true, "Item removed from wishlist.");
        }
    }
}