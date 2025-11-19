using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.BLL.Contracts;

public interface ICartService
{
    Task<Response<CartViewModel>> GetCartAsync(string userId);
    Task<Response<bool>> AddToCartAsync(string userId, Guid productId, int quantity);
    Task<Response<bool>> UpdateItemQuantityAsync(string userId, Guid itemId, int quantity);
    Task<Response<bool>> RemoveFromCartAsync(Guid itemId);
}
