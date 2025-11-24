using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;

namespace BuyMate.BLL.Contracts;

public interface ICheckoutService
{
    Task<Response<CheckoutViewModel>> GetCheckoutViewModelAsync(string userId);
}

