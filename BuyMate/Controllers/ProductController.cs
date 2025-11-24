using BuyMate.BLL.Contracts;
using BuyMate.BLL.Features.Product;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using BuyMate.Infrastructure.Contracts;
using BuyMate.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    // UI-focused Product controller (conventional view discovery like UserController)
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IFileService _fileService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, IFileService fileService, ICategoryService categoryService)
        {
            _productService = productService;
            _fileService = fileService;
            _categoryService = categoryService;
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

        // GET: /Product/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categoriesResponse = await _categoryService.GetAllAsync();

            ViewBag.Categories = categoriesResponse.Data;


            return View(new ProductCreateViewModel()); // resolves Views/Product/Create.cshtml
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model, List<IFormFile>? files)
        {
            if (!ModelState.IsValid)
                return View(model);


            var result = await _productService.CreateAsync(model, files);

            if (result.Status != true)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create product.");
                return View(model);
            }

            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /Product/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var resp = await _productService.GetByIdAsync(id);
            if (!resp.Status || resp.Data == null)
            {
                return NotFound();
            }

            // Map ProductViewModel to ProductUpdateViewModel
            var vm = new ProductUpdateViewModel
            {
                Id = resp.Data.Id,
                Name = resp.Data.Name,
                Description = resp.Data.Description ?? string.Empty,
                Price = resp.Data.OriginalPrice ?? resp.Data.Price,
                DiscountPercentage = resp.Data.Discount,
                StockQuantity = resp.Data.Stock,
                Brand = resp.Data.Brand ?? string.Empty,
                ImageUrls = resp.Data.ImageUrls ?? new List<string>(),
                CategoryIds = resp.Data.Categories?.Select(c => c.Id).ToList() ?? new List<Guid>(),
                Specifications = resp.Data.Specifications?.Select(kv => new BuyMate.DTO.Common.ProductSpecficationInput { Key = kv.Key, Value = kv.Value }).ToList() ?? new List<BuyMate.DTO.Common.ProductSpecficationInput>()
            };

            ViewBag.ProductId = id;
            var categoriesResponse = await _categoryService.GetAllAsync();

            ViewBag.Categories =categoriesResponse.Data;

            return View(vm);
        }

        // POST: /Product/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [FromForm] ProductUpdateViewModel model, List<IFormFile>? files)
        {
            // ensure categories available for re-render
            var categoriesResponse = await _categoryService.GetAllAsync();

            ViewBag.Categories = categoriesResponse.Data;
            ViewBag.ProductId = id;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // If there are uploaded files, save them and append returned URLs to model.ImageUrls
            if (files != null && files.Any())
            {
                var saveResult = await _fileService.SaveImagesAsync(files.ToList(), 4 * 1024 * 1024, new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }, "Products");
                if (!saveResult.Status)
                {
                    ViewBag.Error = saveResult.Message ?? "Failed to save one or more images.";
                    return View(model);
                }

                // Append saved URLs to any existing ImageUrls that were preserved on the client
                model.ImageUrls ??= new List<string>();
                model.ImageUrls.AddRange(saveResult.Data);
            }

            // Call update
            var result = await _productService.UpdateAsync(id, model);
            if (!result.Status)
            {
                ViewBag.Error = result.Message;
                return View(model);
            }

            // Redirect to details or index — here redirect to product details if exists
            return RedirectToAction("Product", new { id = id });
        }

        // POST: /Product/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _productService.DeleteAsync(id);
            TempData["SuccessMessage"] = result.Status ? "Product deleted successfully." : (result.Message ?? "Failed to delete product.");
            return RedirectToAction(nameof(Index));
        }
    }
}
