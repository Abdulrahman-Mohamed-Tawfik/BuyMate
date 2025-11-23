using BuyMate.BLL.Contracts;
using BuyMate.DTO.ViewModels;
using BuyMate.Infrastructure.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    // UI-focused Product controller (conventional view discovery like UserController)
    public class ProductController : Controller
    {
        private readonly IProductService _service;
        public ProductController(IProductService service)
        {
            _service = service;
        }


        // GET: /Product?categoryId=...
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search = null,
            string? orderBy = null,
            bool asc = true,
            Guid? categoryId = null,
            string? brand = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? hasDiscount = null,
            bool? isFeatured = null,
            int pageNumber = 1,
            int pageSize = 12)
        {
            // Paginated query with filters
            var paged = await _service.GetAllPaginatedAsync(pageNumber, pageSize, search, orderBy, asc, categoryId, brand, minPrice, maxPrice, hasDiscount, isFeatured);
            var products = paged.Data ?? new List<ProductViewModel>();

            // Also fetch all products (without filters) to build filter lists like brands and global price range and categories counts
            var allResp = await _service.GetAllAsync();
            var allProducts = allResp.Data ?? new List<ProductViewModel>();

            var categories = allProducts
                .SelectMany(p => p.Categories)
                .GroupBy(c => new { c.Id, c.Name })
                .Select(g => new CategoryViewModel
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    ProductCount = g.Count()
                })
                .ToList();

            var brands = allProducts
                .Where(p => !string.IsNullOrEmpty(p.Brand))
                .Select(p => p.Brand!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(b => b)
                .ToList();

            var vm = new ShopViewModel
            {
                Products = products,
                Categories = categories,
                SelectedCategoryId = categoryId,
                SelectedCategory = categories.FirstOrDefault(c => c.Id == categoryId)?.Name,

                Brands = brands,
                SelectedBrand = brand,

                MinPrice = allProducts.Any() ? allProducts.Min(p => p.Price) : 0,
                MaxPrice = allProducts.Any() ? allProducts.Max(p => p.Price) : 0,
                SelectedMinPrice = minPrice,
                SelectedMaxPrice = maxPrice,

                HasDiscount = hasDiscount,
                IsFeatured = isFeatured,

                OrderBy = orderBy,
                Asc = asc,

                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = paged.TotalCount
            };

            return View(vm); // resolves Views/Product/Index.cshtml
        }

        // GET: /Product/Product/{id}
        [HttpGet]
        public async Task<IActionResult> Product(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Status || result.Data == null) return NotFound();
            return View(result.Data); // resolves Views/Product/Product.cshtml
        }

        // GET: /Product/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ProductCreateViewModel()); // resolves Views/Product/Create.cshtml
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model, List<IFormFile>? files)
        {
            if (!ModelState.IsValid)
                return View(model);


            var result = await _service.CreateAsync(model, files);

            if (result.Status != true)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create product.");
                return View(model);
            }

            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Product/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Status || result.Data == null) return NotFound();

            var p = result.Data;
            var vm = new ProductUpdateViewModel
            {
                Name = p.Name,
                Brand = p.Brand,
                Description = p.Description ?? string.Empty,
                Price = p.Price,
                StockQuantity = p.Stock,
                ImageUrls = p.ImageUrls?.ToList() ?? new List<string>(),
                CategoryIds = p.Categories.Select(c => c.Id).ToList()
            };

            ViewBag.ProductId = id;
            return View(vm); // resolves Views/Product/Edit.cshtml
        }

        // POST: /Product/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductUpdateViewModel model, List<IFormFile>? files)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductId = id;
                return View(model);
            }


            var result = await _service.UpdateAsync(id, model);
            if (!result.Status)
            {
                ViewBag.ProductId = id;
                ViewBag.Error = result.Message ?? "Failed to update product.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction(nameof(Product), new { id });
        }

        // POST: /Product/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            TempData["SuccessMessage"] = result.Status ? "Product deleted successfully." : (result.Message ?? "Failed to delete product.");
            return RedirectToAction(nameof(Index));
        }
    }
}
