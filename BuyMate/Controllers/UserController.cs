using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
