using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    public class UserController : Controller
    {
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
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
    }
}
