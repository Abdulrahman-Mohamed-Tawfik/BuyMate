using BuyMate.BLL.Contracts;
using BuyMate.DTO.Category;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    public class ShopController : Controller
    {
        private readonly ICategoryService _categoryService;

        public ShopController(ICategoryService categoryService /*, other services */)
        {
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Existing actions (Index, Product, Category, etc.) remain here.

        [HttpGet]
        public async Task<IActionResult> Categories(string? q)
        {
            var result = await _categoryService.GetAllAsync();
            var data = result.Data ?? new List<CategoryViewModel>();

            if (!string.IsNullOrWhiteSpace(q))
                data = data
                    .Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.Name)
                    .ToList();
            else
                data = data.OrderBy(c => c.Name).ToList();

            return View(data);
        }
    }
}
