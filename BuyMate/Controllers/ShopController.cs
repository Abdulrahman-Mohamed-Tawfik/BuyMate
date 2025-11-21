using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace BuyMate.Controllers
{
    public class ShopController : Controller
    {
        private readonly HttpClient _client;

        public ShopController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("api");
        }

        // ================================
        // GET: /Shop?categoryId=...
        // ================================
        public async Task<IActionResult> Index(Guid? categoryId = null)
        {
            string url = "products";
            if (categoryId.HasValue)
            {
                url += $"?categoryId={categoryId}";
            }

            var response = await _client
                .GetFromJsonAsync<Response<List<ProductViewModel>>>(url);

            var products = response?.Data ?? new List<ProductViewModel>();

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
                SelectedCategory = categories
                    .FirstOrDefault(c => c.Id == categoryId)?.Name
            };

            return View(vm);
        }

        // ================================
        // GET: /Shop/Product/{id}
        // ================================
        public async Task<IActionResult> Product(Guid id)
        {
            var response = await _client
                .GetFromJsonAsync<Response<ProductViewModel?>>($"products/{id}");

            if (response == null || response.Data == null)
                return NotFound();

            return View(response.Data);
        }

        // ================================
        // GET: /Shop/Create
        // ================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ProductCreateViewModel());
        }

        // ================================
        // POST: /Shop/Create
        // ================================
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _client.PostAsJsonAsync("products", model);

            if (!result.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to create product.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Product created successfully.";
            return RedirectToAction("Index");
        }

        // ================================
        // GET: /Shop/Edit/{id}
        // ================================
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var response = await _client
                .GetFromJsonAsync<Response<ProductViewModel?>>($"products/{id}");

            if (response == null || response.Data == null)
                return NotFound();

            var p = response.Data!;

            var model = new ProductUpdateViewModel
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
            return View(model);
        }

        // ================================
        // POST: /Shop/Edit/{id}
        // ================================
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, ProductUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductId = id;
                return View(model);
            }

            var result = await _client.PutAsJsonAsync($"products/{id}", model);

            if (!result.IsSuccessStatusCode)
            {
                ViewBag.ProductId = id;
                ViewBag.Error = "Failed to update product.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction("Product", new { id });
        }

        // ================================
        // POST: /Shop/Delete/{id}
        // ================================
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _client.DeleteAsync($"products/{id}");

            if (result.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
            else
            {
                TempData["SuccessMessage"] = "Failed to delete product.";
            }

            return RedirectToAction("Index");
        }
    }
}
