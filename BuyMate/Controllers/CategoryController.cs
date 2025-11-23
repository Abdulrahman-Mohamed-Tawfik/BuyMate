using BuyMate.BLL.Contracts;
using BuyMate.DTO.Category;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    // This ONE controller now handles everything for Categories
    public class CategoryController : Controller
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        // ==========================
        //  WEB PAGES (For Humans) 🖥️
        // ==========================

        // 1. LIST PAGE
        public async Task<IActionResult> Index()
        {
            var categories = await _service.GetAllAsync();
            // Looks for Views/Home/CategoryListing.cshtml as you requested
            return View("~/Views/Home/CategoryListing.cshtml", categories);
        }

        // 2. CREATE PAGE (The Form)
        [HttpGet]
        public IActionResult Create()
        {
            return View(); // Looks for Views/Category/Create.cshtml (or Home if you moved it)
        }

        // 3. CREATE ACTION (Saves the data)
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // 4. EDIT PAGE (The Form)
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();

            var dto = new CreateCategoryDto { Name = category.Name };
            ViewBag.Id = category.Id;

            return View("~/Views/Home/Edit.cshtml", dto);
        }

        // 5. EDIT ACTION (Saves updates)
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, CreateCategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                var success = await _service.UpdateAsync(id, dto);
                if (!success) return NotFound();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Id = id;
            return View(dto);
        }

        // 6. DELETE ACTION
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

   
    }
}