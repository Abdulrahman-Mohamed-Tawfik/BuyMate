using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    [Route("upload")]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string folder = Path.Combine(_env.WebRootPath, "images/products");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // URL النهائي اللي هترجع بيه الصورة
            string imageUrl = $"{Request.Scheme}://{Request.Host}/images/products/{fileName}";

            return Ok(new { imageUrl });
        }
    }
}
