using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.Cart;
using BuyMate.DTO.ViewModels.Wishlist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BuyMate.BLL.Contracts
{
    public interface IWishlistService
    {
        Task<Response<WishlistViewModel>> GetWishlistAsync(string userId);
        Task<Response<bool>> AddtoWishlistAsync(string userId, Guid productId);
        Task<Response<bool>> RemoveFromWishlistAsync(string userId, Guid itemId);
        Task<Response<bool>> ClearWishlistAsync(string userId);
    }
}