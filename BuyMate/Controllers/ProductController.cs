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

        //GET: /Product
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var filter = new ProductFilter
            {
                PageNumber = 1,
                PageSize = 20
            };
            var productsResponse = await _productService.GetAllPaginatedAsync(filter);
            var products = productsResponse.Data ?? new List<ProductViewModel>();
            return View(products); // resolves Views/Product/Index.cshtml
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

            var categoriesResponse = await _categoryService.GetAllAsync();
            ViewBag.Categories = categoriesResponse.Data;

            if (!ModelState.IsValid)
            {
                return View(model);
            }
                
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
                StockQuantity = resp.Data.StockQuantity,
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

          
            // Call update
            var result = await _productService.UpdateAsync(id, model, files);
            if (!result.Status)
            {
                ViewBag.Error = result.Message;
                return View(model);
            }

            return RedirectToAction("Index");
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
