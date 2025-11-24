using BuyMate.BLL.Contracts;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.ViewComponents;

public class CartSummaryViewComponent : ViewComponent
{
    private readonly ICartService _cartService;
    private readonly IUserProfileService _userProfileService;
    public CartSummaryViewComponent(ICartService cartService, IUserProfileService userProfileService)
    {
        _cartService = cartService;
        _userProfileService = userProfileService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Return empty summary for anonymous users
        if (!(HttpContext?.User?.Identity?.IsAuthenticated ?? false))
        {
            return View(new MiniCartViewModel { TotalItems = 0, TotalPrice = 0 });
        }

        var profile = await _userProfileService.GetProfileAsync(HttpContext.User);
        if (profile?.Status != true || profile.Data is null || string.IsNullOrWhiteSpace(profile.Data.Id))
        {
            return View(new MiniCartViewModel { TotalItems = 0, TotalPrice = 0 });
        }

        var cartVm = await _cartService.GetCartAsync(profile.Data.Id);
        if (cartVm?.Status != true || cartVm.Data is null)
        {
            return View(new MiniCartViewModel { TotalItems = 0, TotalPrice = 0 });
        }

        var miniCartVm = new MiniCartViewModel
        {
            TotalItems = cartVm.Data.Items?.Sum(i => i.Quantity) ?? 0,
            TotalPrice = cartVm.Data.Total
        };

        return View(miniCartVm);
    }
}
