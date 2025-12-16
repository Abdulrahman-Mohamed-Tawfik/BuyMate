using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.Order;

namespace BuyMate.BLL.Contracts;

public interface ICheckoutService
{
    Task<Response<CheckoutViewModel>> GetCheckoutViewModelAsync(string userId);
}

