using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
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
        public async Task<IActionResult> Index(ProductFilter? filter = null)
        {

            // Paginated query with filters
            var paged = await _service.GetAllPaginatedAsync(filter);
            var products = paged.Data ?? new List<ProductViewModel>();

           

            //get all categories
            var categories = await _service.GetAllCategoriesAsync();
            var brands = await _service.GetAllBrandsAsync();

            var vm = new ShopViewModel
            {
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
