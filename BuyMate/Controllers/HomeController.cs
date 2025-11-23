using System.Diagnostics;
using BuyMate.BLL.Contracts;
using BuyMate.DTO.ViewModels;
using BuyMate.Model;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryService _categoryService;

        public HomeController(ILogger<HomeController> logger, ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categoriesDto = await _categoryService.GetAllAsync();
            var categories = categoriesDto.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                ProductCount = 0
            }).ToList();

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
