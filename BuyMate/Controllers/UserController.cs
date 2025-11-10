using BuyMate.BLL.Contracts;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BuyMate.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthService _authService;

        public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Profile()
        {
            var userViewModel = new ProfileViewModel
            {
                Name = "Abdulrahman Mohamed",
                Email = "abdulrahman.mohamed@example.com",
                Phone = "123-456-7890",
                Avatar = "/images/avatars/default.png"

            };
            return View(userViewModel);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model);
            if (result.Status == true)
            {
                TempData["Success"] = "Account created successfully. Please sign in.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Data?.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
