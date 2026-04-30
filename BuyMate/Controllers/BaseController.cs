using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    public abstract class BaseController : Controller
    {
        protected string UserId
        {
            get
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User ID claim is missing");

                return userId;
            }
        }
        protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

        protected IActionResult RedirectToLogin() => RedirectToAction("Login", "User");

        protected void SetErrorMessage(string message) => TempData["Error"] = message;

        protected void SetSuccessMessage(string message) => TempData["Success"] = message;
    }
}
