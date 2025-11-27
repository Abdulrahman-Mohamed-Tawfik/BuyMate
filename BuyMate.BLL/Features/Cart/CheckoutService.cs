using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;

namespace BuyMate.BLL.Features.Cart;

public class CheckoutService : ICheckoutService
{
    private readonly ICartService _cartService;
    public CheckoutService(ICartService cartService)
    {
        _cartService = cartService;
    }
    public async Task<Response<CheckoutViewModel>> GetCheckoutViewModelAsync(string userId)
    {
        var cartResponse = await _cartService.GetCartAsync(userId);
        if (cartResponse.Status is false)
        {
            return new Response<CheckoutViewModel>
            {
                Status = false,
                Message = "Your cart is empty.",
                Data = null
            };
        }
        var checkoutViewModel = new CheckoutViewModel
        {
            CartVm = cartResponse.Data!
        };
        return new Response<CheckoutViewModel>
        {
            Status = true,
            Message = "Checkout data retrieved successfully.",
            Data = checkoutViewModel
        };
    }
}

