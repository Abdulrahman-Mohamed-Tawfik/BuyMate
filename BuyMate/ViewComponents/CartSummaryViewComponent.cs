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
        var profile = await _userProfileService.GetProfileAsync(HttpContext.User);
        var cartVm = await _cartService.GetCartAsync(profile.Data!.Id);

        if (cartVm.Status is false || profile.Status is false)
        {
            return View(new MiniCartViewModel
            {
                TotalItems = 0,
                TotalPrice = 0
            });
        }

        var miniCartVm = new MiniCartViewModel
        {
            TotalItems = cartVm.Data!.Items.Sum(i => i.Quantity),
            TotalPrice = cartVm.Data.Total
        };
        return View(miniCartVm);
    }
}
