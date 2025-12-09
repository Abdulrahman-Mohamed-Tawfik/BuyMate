using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.Category;
using BuyMate.DTO.ViewModels.Product;
using BuyMate.DTO.ViewModels.Shop;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    public class ShopController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public ShopController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
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

        // GET: /Product?categoryId=...
        [HttpGet]
        public async Task<IActionResult> Index(ProductFilter? filter = null)
        {

            // Paginated query with filters
            var paged = await _productService.GetAllPaginatedAsync(filter);
            var products = paged.Data ?? new List<ProductViewModel>();



            //get all categories
            var categoriesResponse = await _categoryService.GetAllAsync();
            var categories = categoriesResponse.Data;
            var brands = await _productService.GetAllBrandsAsync();

            var vm = new ShopViewModel
            {
                Search = filter?.Search,
                Products = products,
                Categories = categories,
                SelectedCategoryId = filter.CategoryId,
                SelectedCategory = categories.FirstOrDefault(c => c.Id == filter.CategoryId)?.Name,

                Brands = brands,
                SelectedBrand = filter.Brand,

                MinPrice = filter.MinPrice ?? 0,
                MaxPrice = filter.MaxPrice ?? 0,
                SelectedMinPrice = filter.MinPrice,
                SelectedMaxPrice = filter.MaxPrice,

                HasDiscount = filter.HasDiscount,
                IsFeatured = filter.IsFeatured,

                OrderBy = filter.OrderBy,
                Asc = filter.Asc,

                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = paged.TotalCount
            };

            return View(vm); // resolves Views/Product/Index.cshtml
        }

        // GET: /Product/Product/{id}
        [HttpGet]
        public async Task<IActionResult> Product(Guid id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (!result.Status || result.Data == null) return NotFound();
            return View(result.Data); // resolves Views/Product/Product.cshtml
        }

        // shop/deals
        [HttpGet]
        public async Task<IActionResult> Deals()
        {
            var paged = await _productService.GetAllPaginatedAsync(new ProductFilter
            {
                HasDiscount = true,
                IsFeatured = null,
                PageNumber = 1,
                PageSize = 20
            });

            var products = paged.Data ?? new List<ProductViewModel>();
            return View(products); // resolves Views/Shop/Deals.cshtml expecting List<ProductViewModel>
        }
    }
}
