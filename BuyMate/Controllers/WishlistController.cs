using BuyMate.BLL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    [Authorize]
    public class WishlistController : BaseController
    {
        private readonly IWishlistService _wishlistService;
        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var wishlistVm = await _wishlistService.GetWishlistAsync(UserId);
            return View(wishlistVm.Data);
        }

        [HttpPost]
        public async Task<IActionResult> AddtoWishlist(Guid productId)
        {
            var result = await _wishlistService.AddtoWishlistAsync(UserId, productId);
            if (result.Status is false)
            {
                return BadRequest(new { success = false, message = result.Message });
            }
            return Ok(new { success = true, message = result.Message });
        }
        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(Guid itemId)
        {
            var response = await _wishlistService.RemoveFromWishlistAsync(UserId,itemId);
            if (response.Status)
            {
                return Ok(new { success = true, message = response.Message });
            }
            return BadRequest(new { success = false, message = response.Message });
        }
    }
}
