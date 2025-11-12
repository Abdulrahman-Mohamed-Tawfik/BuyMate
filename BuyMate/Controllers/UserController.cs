using BuyMate.BLL.Contracts;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BuyMate.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserProfileService _userProfileService;

        public UserController(IAuthService authService,IUserProfileService userProfileService)
        {
            _authService = authService;
            _userProfileService = userProfileService;
        }
        /*
        public IActionResult Index()
        {
            return View();
        }*/

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var result = await _userProfileService.GetProfileAsync(User);
            if (result.Status == null)
            {
                //TempData["Error"] = "User not found. Please login again.";
                return RedirectToAction("Login");
            }

            return View(result.Data);

            
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

          
            ModelState.AddModelError(string.Empty, result.Message);
            

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await _authService.LoginAsync(model);

            if (result.Status == true)
            {
                //TempData["Success"] = "Login Successful.";

                return RedirectToAction("Index", "Home");
            }

           
           ModelState.AddModelError(string.Empty, result.Message);
            

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "You are not logged in.";
                return RedirectToAction("Login");
            }

            var result = await _authService.LogoutAsync();

            if (result.Status == true)
            {
                //TempData["Success"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Logout failed.";
            return RedirectToAction("Profile");
        }


    }
}
