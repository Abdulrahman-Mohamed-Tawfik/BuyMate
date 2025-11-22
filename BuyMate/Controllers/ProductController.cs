using BuyMate.BLL.Contracts;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    // UI-focused Product controller (conventional view discovery like UserController)
    public class ProductController : Controller
    {
        private readonly IProductService _service;
        private readonly IWebHostEnvironment _env;

        public ProductController(IProductService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // GET: /Product?categoryId=...
        [HttpGet]
        public async Task<IActionResult> Index(Guid? categoryId = null)
        {
            var response = await _service.GetAllAsync(null, null, true, categoryId);
            var products = response.Data ?? new List<ProductViewModel>();

            var categories = products
                .SelectMany(p => p.Categories)
                .GroupBy(c => new { c.Id, c.Name })
                .Select(g => new CategoryViewModel
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    ProductCount = g.Count()
                })
                .ToList();

            var vm = new ShopViewModel
            {
                Products = products,
                Categories = categories,
                SelectedCategoryId = categoryId,
                SelectedCategory = categories.FirstOrDefault(c => c.Id == categoryId)?.Name
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

            // Handle file saving similar to avatar approach
            if (files != null && files.Any())
            {
                string folder = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;
                    var ext = Path.GetExtension(file.FileName);
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    if (!allowed.Contains(ext, StringComparer.OrdinalIgnoreCase)) continue;

                    string fileName = Guid.NewGuid().ToString("N") + ext.ToLowerInvariant();
                    string path = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    model.ImageUrls.Add($"/images/products/{fileName}");
                }
            }

            var result = await _service.CreateAsync(model);
            if (!result.Status)
            {
                ViewBag.Error = result.Message ?? "Failed to create product.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Product created successfully.";
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

            // Replace images if new ones uploaded
            if (files != null && files.Any())
            {
                model.ImageUrls.Clear();
                string folder = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;
                    var ext = Path.GetExtension(file.FileName);
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    if (!allowed.Contains(ext, StringComparer.OrdinalIgnoreCase)) continue;

                    string fileName = Guid.NewGuid().ToString("N") + ext.ToLowerInvariant();
                    string path = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    model.ImageUrls.Add($"/images/products/{fileName}");
                }
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
