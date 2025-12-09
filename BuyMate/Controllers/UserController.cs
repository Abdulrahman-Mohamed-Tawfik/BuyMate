using BuyMate.BLL.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using BuyMate.DTO.ViewModels.User;

namespace BuyMate.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserProfileService _userProfileService;
        private readonly UserManager<BuyMate.Model.Entities.User> _userManager;

        public UserController(IAuthService authService, IUserProfileService userProfileService, UserManager<BuyMate.Model.Entities.User> userManager)
        {
            _authService = authService;
            _userProfileService = userProfileService;
            _userManager = userManager;
        }
      

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var result = await _userProfileService.GetProfileAsync(User);
            if (result.Status == false)
            {
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
            var result = await _authService.LoginMvcAsync(model);

            if (result.Status == true)
            {
                return RedirectToAction("Index", "Home");
            }


            ModelState.AddModelError(string.Empty, result.Message);


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity != null && !User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "You are not logged in.";
                return RedirectToAction("Login");
            }

            var result = await _authService.LogoutAsync();

            if (result.Status == true)
            {
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Logout failed.";
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var result = await _userProfileService.GetProfileAsync(User);
            if (result.Status != true || result.Data == null)
            {
                return RedirectToAction("Login");
            }


            return View(result.Data);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ProfileViewModel model, IFormFile? avatarFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userProfileService.UpdateProfileAsync(model, User, avatarFile);
            if (result.Status != true)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }
            
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        // API endpoint for login (for Flutter/Angular/MAUI/etc.)
        [AllowAnonymous]
        [HttpPost("api/login")]
        public async Task<IActionResult> ApiLogin([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid login payload.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var result = await _authService.LoginApiAsync(model);
            if (!result.Status)
            {
                return Unauthorized(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, token = result.Data });
        }

    }
}
