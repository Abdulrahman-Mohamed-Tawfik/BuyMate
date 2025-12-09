using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.Order;

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
            return Response<CheckoutViewModel>.Fail("Your cart is empty.");

        var checkoutViewModel = new CheckoutViewModel
        {
            CartVm = cartResponse.Data!
        };

        return Response<CheckoutViewModel>.Success(checkoutViewModel, "Checkout data retrieved successfully.");
    }
}

