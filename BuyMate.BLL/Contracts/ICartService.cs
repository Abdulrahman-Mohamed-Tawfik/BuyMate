using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;

namespace BuyMate.BLL.Contracts;

public interface ICartService
{
    Task<Response<CartViewModel>> GetCartAsync(string userId);
    Task<Response<bool>> AddToCartAsync(string userId, Guid productId, int quantity);
    Task<Response<bool>> UpdateItemQuantityAsync(string userId, Guid itemId, int quantity);
    Task<Response<bool>> RemoveFromCartAsync(Guid itemId);
}
