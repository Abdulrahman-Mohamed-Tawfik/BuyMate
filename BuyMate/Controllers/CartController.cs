using BuyMate.BLL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly ICheckoutService _checkoutService;
    private readonly IUserProfileService _userProfileService;
    public CartController(ICartService cartService, ICheckoutService checkoutService, IUserProfileService userProfileService)
    {
        _cartService = cartService;
        _checkoutService = checkoutService;
        _userProfileService = userProfileService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var profile = await _userProfileService.GetProfileAsync(User);
        if (profile.Status is false || profile.Data is null)
            return RedirectToAction("Login", "User");

        var cartVm = await _cartService.GetCartAsync(profile.Data.Id);
        return View(cartVm.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
    {
        var profile = await _userProfileService.GetProfileAsync(User);
        if (profile.Status is false)
        {
            return Unauthorized(profile.Message);
        }
        var result = await _cartService.AddToCartAsync(profile.Data!.Id, productId, quantity);
        if (result.Status is false)
        {
            return BadRequest(new { success = false, message = result.Message });
        }
        var cartResult = await _cartService.GetCartAsync(profile.Data!.Id);
        var newCount = cartResult.Data?.Items.Sum(i => i.Quantity) ?? 0;
        var totalPrice = cartResult.Data?.Total ?? 0;

        return Ok(new { success = true, message = result.Message, newCount, totalPrice });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(Guid itemId, int quantity)
    {
        var profile = await _userProfileService.GetProfileAsync(User);
        if (profile.Status is false)
        {
            return RedirectToAction("Login", "User");
        }

        var response = await _cartService.UpdateItemQuantityAsync(profile.Data!.Id, itemId, quantity);

        if (!response.Status)
        {
            TempData["Error"] = response.Message;
        }
        else
        {
            TempData["Success"] = "Cart updated.";
        }

        return RedirectToAction("Index");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid itemId)
    {
        var response = await _cartService.RemoveFromCartAsync(itemId);

        if (response.Status)
        {
            TempData["Success"] = response.Message;
        }
        else
        {
            TempData["Error"] = response.Message;
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var profile = await _userProfileService.GetProfileAsync(User);
        if (profile.Status is false)
        {
            return RedirectToAction("Login", "User");
        }
        var checkoutVmResult = await _checkoutService.GetCheckoutViewModelAsync(profile.Data!.Id);
        if (checkoutVmResult.Status is false)
        {
            TempData["Error"] = checkoutVmResult.Message;
            return RedirectToAction("Index");
        }
        return View(checkoutVmResult.Data);
    }

}
