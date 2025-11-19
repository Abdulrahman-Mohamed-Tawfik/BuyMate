using BuyMate.BLL.Contracts;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BuyMate.Controllers
{
    [Route("products")]
    public class ProductController : Controller
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        // ====================================================
        //   NEW Pagination Endpoint
        // ====================================================
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            int pageNumber = 1,
            int pageSize = 10,
            string? q = null,
            string? orderBy = null,
            bool asc = true)
        {
            var result = await _service.GetAllPaginatedAsync(pageNumber, pageSize, q, orderBy, asc);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? orderBy, [FromQuery] bool asc = true, [FromQuery] Guid? categoryId = null)
        {
            var result = await _service.GetAllAsync(q, orderBy, asc, categoryId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Status || result.Data == null) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.CreateAsync(model);
            return CreatedAtAction(nameof(Details), new { id = result.Data }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(id, model);
            if (!result.Status) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Status) return NotFound(result);
            return Ok(result);
        }
    }
}
