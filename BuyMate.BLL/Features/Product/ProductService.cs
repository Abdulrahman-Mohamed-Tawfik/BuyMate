using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Repositories;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using BuyMate.Model.Entities;
using Microsoft.EntityFrameworkCore;

// ⭐ مهم جداً
using ProductEntity = BuyMate.Model.Entities.Product;

namespace BuyMate.BLL.Features.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IProductImageRepository _imageRepo;

        public ProductService(IProductRepository repo, IProductImageRepository imageRepo)
        {
            _repo = repo;
            _imageRepo = imageRepo;
        }

        public async Task<PaginatedResponse<List<ProductViewModel>>> GetAllPaginatedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            string? orderBy = null,
            bool asc = true)
        {
            var query = await _repo.SearchAsync(search);

            query = _repo.OrderBy(query, orderBy, asc)
                         .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                         .Include(p => p.Reviews);

            int totalCount = await query.CountAsync();

            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var ids = products.Select(x => x.Id).ToList();
            var images = await _imageRepo.GetAllQueryable(i => ids.Contains(i.ProductId)).ToListAsync();

            var groupedImages = images.GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var list = products.Select(p =>
            {
                var imgs = groupedImages.TryGetValue(p.Id, out var gi) ? gi : new List<ProductImage>();
                var cats = p.ProductCategories.Select(pc => pc.Category!).ToList();
                return ToViewModel(p, imgs, cats);
            }).ToList();

            return new PaginatedResponse<List<ProductViewModel>>
            {
                Data = list,
                Message = "Products",
                Status = true,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Response<List<ProductViewModel>>> GetAllAsync(string? search = null, string? orderBy = null,
            bool asc = true, Guid? categoryId = null)
        {
            var query = categoryId.HasValue
                ? await _repo.FilterByCategoryAsync(categoryId.Value)
                : await _repo.SearchAsync(search);

            query = _repo.OrderBy(query, orderBy, asc)
                         .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                         .Include(p => p.Reviews);

            var products = await query.ToListAsync();

            var ids = products.Select(p => p.Id).ToList();
            var allImages = await _imageRepo.GetAllQueryable(i => ids.Contains(i.ProductId)).ToListAsync();

            var imagesByProduct = allImages
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var list = products.Select(p =>
            {
                var imgs = imagesByProduct.TryGetValue(p.Id, out var gi) ? gi : new List<ProductImage>();
                var cats = p.ProductCategories.Select(pc => pc.Category!).ToList();
                return ToViewModel(p, imgs, cats);
            }).ToList();

            return new Response<List<ProductViewModel>>
            {
                Data = list,
                Status = true,
                Message = "Products"
            };
        }

        public async Task<Response<ProductViewModel?>> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return new Response<ProductViewModel?>
                {
                    Data = null,
                    Status = false,
                    Message = "Not Found"
                };

            var images = await _imageRepo.GetByProductIdAsync(id);
            var cats = entity.ProductCategories.Select(pc => pc.Category!).ToList();

            return new Response<ProductViewModel?>
            {
                Data = ToViewModel(entity, images, cats),
                Status = true,
                Message = "Product"
            };
        }

        public async Task<Response<Guid>> CreateAsync(ProductCreateViewModel model)
        {
            var entity = new ProductEntity
            {
                Name = model.Name,
                Brand = model.Brand,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity
            };

            var created = await _repo.CreateAsync(entity);

            if (model.ImageUrls.Any())
            {
                var images = model.ImageUrls.Select((url, index) => new ProductImage
                {
                    ProductId = created.Id,
                    ImageUrl = url,
                    IsMain = index == 0
                }).ToList();

                await _imageRepo.CreateRangeAsync(images);
            }

            if (model.CategoryIds.Any())
                await _repo.AddProductCategoriesAsync(created.Id, model.CategoryIds);

            return new Response<Guid>
            {
                Data = created.Id,
                Status = true,
                Message = "Created"
            };
        }

        public async Task<Response<bool>> UpdateAsync(Guid id, ProductUpdateViewModel model)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return new Response<bool> { Status = false, Data = false, Message = "Not Found" };

            entity.Name = model.Name;
            entity.Brand = model.Brand;
            entity.Description = model.Description;
            entity.Price = model.Price;
            entity.StockQuantity = model.StockQuantity;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);

            if (model.ImageUrls.Any())
            {
                await _imageRepo.RemoveByProductIdAsync(id);

                var imgs = model.ImageUrls.Select((url, index) => new ProductImage
                {
                    ProductId = id,
                    ImageUrl = url,
                    IsMain = index == 0
                }).ToList();

                await _imageRepo.CreateRangeAsync(imgs);
            }

            await _repo.RemoveProductCategoriesAsync(id);

            if (model.CategoryIds.Any())
                await _repo.AddProductCategoriesAsync(id, model.CategoryIds);

            return new Response<bool>
            {
                Data = true,
                Status = true,
                Message = "Updated"
            };
        }

        public async Task<Response<bool>> DeleteAsync(Guid id)
        {
            var ok = await _repo.SoftDeleteAsync(id);

            return new Response<bool>
            {
                Data = ok,
                Status = ok,
                Message = ok ? "Deleted" : "Not Found"
            };
        }

        private static ProductViewModel ToViewModel(ProductEntity p, List<ProductImage> images, List<Category> categories)
{
    var rating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0.0;

    var mainImage =
        images.FirstOrDefault(i => i.IsMain)?.ImageUrl ??
        images.FirstOrDefault()?.ImageUrl;

    return new ProductViewModel
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Brand = p.Brand,
        // 🔹 لو فيه DiscountPrice نحسب OriginalPrice و % الخصم
        OriginalPrice = 7000,
        Discount = 10,
        

        // 🔹 قيمة ثابتة مؤقتًا
        IsFlashDeal = true,

        // 🔹 Specifications مش موجودة → نخليها null
        Specifications = null,

        // باقي البيانات
        ImageUrl = mainImage,
        ImageUrls = images.Select(i => i.ImageUrl).ToList(),
        Categories = categories.Select(c => new CategoryViewModel { Id = c.Id, Name = c.Name }).ToList(),
        Rating = rating,
        ReviewCount = p.Reviews.Count,
        Stock = p.StockQuantity
    };
}

    }
}
