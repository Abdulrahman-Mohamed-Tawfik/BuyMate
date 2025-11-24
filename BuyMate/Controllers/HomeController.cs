using System.Diagnostics;
using BuyMate.BLL.Contracts;
using BuyMate.DTO.Category;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using BuyMate.Model;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public HomeController(ILogger<HomeController> logger,ICategoryService categoryService, IProductService productService)
        {
            _logger = logger;
            _categoryService = categoryService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var categoriesDto = await _categoryService.GetAllAsync();
            var categories = categoriesDto?.Data;


            var filter = new ProductFilter
            {
                IsFeatured = true,
                PageNumber = 1,
                PageSize = 4
            };
            var featured = _productService.GetAllPaginatedAsync(filter).Result.Data;

            var bestSellers = new List<ProductViewModel>
        {
            new ProductViewModel {
                Id = Guid.NewGuid(),
                Name = "Wireless Mouse",
                Price = 25,
                ImageUrl = "Products/mouse.jpg",
                IsBestSeller = true
            },
            new ProductViewModel {
                Id = Guid.NewGuid(),
                Name = "Bluetooth Speaker",
                Price = 80,
                ImageUrl = "Products/speaker.jpg",
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

        public IActionResult AccessDenied()
        {
            return View();
        }

        // Keep old name for compatibility, redirect to Terms view
        public IActionResult TermsOfService()
        {
            return View("Terms");
        }

        // New explicit actions for pages used in layout
        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Support()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Cookies()
        {
            return View();
        }

    }
}
