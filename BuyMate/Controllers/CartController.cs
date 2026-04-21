using BuyMate.BLL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers;

[Authorize]
public class CartController : BaseController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cartVm = await _cartService.GetCartAsync(UserId);
        return View(cartVm.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
    {
        var result = await _cartService.AddToCartAsync(UserId, productId, quantity);

        if (IsAjaxRequest())
        {
            if (!result.Status) return BadRequest(new { success = false, message = result.Message });

            var cartResult = await _cartService.GetCartAsync(UserId);
            var newCount = cartResult.Data?.Items.Sum(i => i.Quantity) ?? 0;
            var totalPrice = cartResult.Data?.Total ?? 0;

            return Ok(new { success = true, message = result.Message, newCount, totalPrice });
        }

        if (!result.Status)
        {
            SetErrorMessage(result.Message);
        }
        else
        {
            SetSuccessMessage(result.Message ?? "Item added to cart.");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(Guid itemId, int quantity)
    {
        var response = await _cartService.UpdateItemQuantityAsync(UserId, itemId, quantity);

        if (IsAjaxRequest())
        {
            if (!response.Status) return BadRequest(new { success = false, message = response.Message });

            var cartResult = await _cartService.GetCartAsync(UserId);
            var updatedItem = cartResult.Data?.Items.FirstOrDefault(i => i.ItemId == itemId);
            var newCount = cartResult.Data?.Items.Sum(i => i.Quantity) ?? 0;

            return Ok(new { 
                success = true, 
                message = "Cart updated.",
                itemTotal = updatedItem?.TotalPrice ?? 0,
                subtotal = cartResult.Data?.Subtotal ?? 0,
                total = cartResult.Data?.Total ?? 0,
                newCount
            });
        }

        if (!response.Status)
        {
            SetErrorMessage(response.Message);
        }
        else
        {
            SetSuccessMessage("Cart updated.");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid itemId)
    {
        var response = await _cartService.RemoveFromCartAsync(itemId);

        if (IsAjaxRequest())
        {
            if (!response.Status) return BadRequest(new { success = false, message = response.Message });
            return Ok(new { success = true, message = response.Message });
        }

        if (!response.Status)
        {
            SetErrorMessage(response.Message);
        }
        else
        {
            SetSuccessMessage(response.Message);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool IsAjaxRequest()
    {
        return Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
               Request.Headers["Accept"].ToString().Contains("application/json");
    }
}
