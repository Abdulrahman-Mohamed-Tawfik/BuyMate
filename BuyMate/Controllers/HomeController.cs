using System.Diagnostics;
using BuyMate.DTO.ViewModels;
using BuyMate.Model;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var categories = new List<CategoryViewModel>
        {
            new CategoryViewModel { Id = Guid.NewGuid(), Name = "Electronics" },
            new CategoryViewModel { Id = Guid.NewGuid(), Name = "Fashion" },
            new CategoryViewModel { Id = Guid.NewGuid(), Name = "Books" }
        };

            var featured = new List<ProductViewModel>
        {
            new ProductViewModel {
                Id = Guid.NewGuid(),
                Name = "iPhone 15",
                Price = 1200,
                ImageUrl = "iphone.jpeg",
                IsFeatured = true
            },
            new ProductViewModel {
                Id = Guid.NewGuid(),
                Name = "Gaming Laptop",
                Price = 2500,
                ImageUrl = "laptop.jpg",
                IsFeatured = true
            }
        };

            var bestSellers = new List<ProductViewModel>
        {
            new ProductViewModel {
                Id = Guid.NewGuid(),
                Name = "Wireless Mouse",
                Price = 25,
                ImageUrl = "mouse.jpg",
                IsBestSeller = true
            },
            new ProductViewModel {
                Id = Guid.NewGuid(),
                Name = "Bluetooth Speaker",
                Price = 80,
                ImageUrl = "speaker.jpg",
                IsBestSeller = true
            }
        };

            var home = new HomeViewModel
            {
                Categories = categories,
                FeaturedProducts = featured,
                BestSellers = bestSellers
            };

            return View(home);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

       
    }
}
