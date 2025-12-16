using BuyMate.BLL.Contracts;
using BuyMate.DTO.Category;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    [Authorize(Roles = "admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _categoryService.GetAllAsync();
            if (!result.Status)
            {
                TempData["Error"] = result.Message ?? "Failed to load categories.";
                return View(new List<CategoryViewModel>());
            }

            return View(result.Data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryDto dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _categoryService.CreateAsync(dto, imageFile);
            if (result.Status)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create category.");
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            if (!result.Status || result.Data == null)
                return NotFound();

            var dto = new CreateCategoryDto { Name = result.Data.Name };
            ViewBag.Id = result.Data.Id;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CreateCategoryDto dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Id = id;
                return View(dto);
            }

            var result = await _categoryService.UpdateAsync(id, dto, imageFile);
            if (result.Status)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            if (result.Message != null)
                ModelState.AddModelError(string.Empty, result.Message);

            ViewBag.Id = id;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _categoryService.DeletePhysicalAsync(id);
            if (result.Status)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message ?? "Delete failed.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}