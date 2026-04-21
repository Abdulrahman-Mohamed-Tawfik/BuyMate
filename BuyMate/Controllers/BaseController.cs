using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    public abstract class BaseController : Controller
    {
        protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

        protected IActionResult RedirectToLogin() => RedirectToAction("Login", "User");

        protected void SetErrorMessage(string message) => TempData["Error"] = message;

        protected void SetSuccessMessage(string message) => TempData["Success"] = message;
    }
}
