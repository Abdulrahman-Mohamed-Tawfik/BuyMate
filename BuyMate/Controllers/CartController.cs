using BuyMate.BLL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BuyMate.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IUserProfileService _userProfileService;
    public CartController(ICartService cartService, IUserProfileService userProfileService)
    {
        _cartService = cartService;
        _userProfileService = userProfileService;
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var profile = await _userProfileService.GetProfileAsync(User);
        if (profile.Status is false)
        {
            return RedirectToAction("Login", "User");
        }

        var cartVm = await _cartService.GetCartAsync(profile.Data!.Id);
        if(cartVm.Status is false)
        {
            //TODO: Return a view with empty cart
            return BadRequest(cartVm.Message);
        }
        return View(cartVm.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Add(Guid productId, int quantity = 1)
    {
        var profile = await _userProfileService.GetProfileAsync(User);
        if (profile.Status is false)
        {
            return RedirectToAction("Login", "User");
        }
        var result = await _cartService.AddToCartAsync(profile.Data!.Id, productId, quantity);
        if (result.Status is false)
        {
            return BadRequest(result.Message);
        }
        return RedirectToAction("Index", "Cart");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
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
    [Authorize]
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

}
