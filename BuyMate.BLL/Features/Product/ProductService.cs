using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Repositories;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using BuyMate.Infrastructure.Contracts;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

// ⭐ مهم جداً
using ProductEntity = BuyMate.Model.Entities.Product;

namespace BuyMate.BLL.Features.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IProductImageRepository _imageRepo;
        private readonly IFileService _fileService;


        // Configuration constants
        private const string ProductsFolderName = "Products";
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 4 * 1024 * 1024; //4 MB
        public ProductService(IProductRepository repo, IProductImageRepository imageRepo, IFileService fileService)
        {
            _repo = repo;
            _imageRepo = imageRepo;
            _fileService = fileService;
        }

        public async Task<PaginatedResponse<List<ProductViewModel>>> GetAllPaginatedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            string? orderBy = null,
            bool asc = true,
            Guid? categoryId = null,
            string? brand = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? hasDiscount = null,
            bool? isFeatured = null)
        {
            var query = categoryId.HasValue
                ? await _repo.FilterByCategoryAsync(categoryId.Value)
                : await _repo.SearchAsync(search);

            // apply additional filters
            query = ApplyFilters(query, brand, minPrice, maxPrice, hasDiscount, isFeatured);

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
            bool asc = true, Guid? categoryId = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, bool? hasDiscount = null, bool? isFeatured = null)
        {
            var query = categoryId.HasValue
                ? await _repo.FilterByCategoryAsync(categoryId.Value)
                : await _repo.SearchAsync(search);

            // apply additional filters
            query = ApplyFilters(query, brand, minPrice, maxPrice, hasDiscount, isFeatured);

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

        private static IQueryable<ProductEntity> ApplyFilters(IQueryable<ProductEntity> query, string? brand, decimal? minPrice, decimal? maxPrice, bool? hasDiscount, bool? isFeatured)
        {
            if (!string.IsNullOrWhiteSpace(brand))
            {
                var b = brand.Trim();
                query = query.Where(p => p.Brand != null && p.Brand.ToLower() == b.ToLower());
            }

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (hasDiscount.HasValue)
            {
                if (hasDiscount.Value)
                    query = query.Where(p => p.DiscountPercentage.HasValue && p.DiscountPercentage.Value > 0);
                else
                    query = query.Where(p => !p.DiscountPercentage.HasValue || p.DiscountPercentage.Value <= 0);
            }

            if (isFeatured.HasValue)
            {
                var fourteenDaysAgo = DateTime.UtcNow.AddDays(-14);
                if (isFeatured.Value)
                    query = query.Where(p =>
                        // use the same logic as IsFeatured private method
                        p.CreatedAt>=fourteenDaysAgo
                    );
                else
                    query = query.Where(p => p.CreatedAt<fourteenDaysAgo);
            }

            return query;
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

            // Load related data that may not have been included by repository
            var images = await _imageRepo.GetByProductIdAsync(id);

            // Ensure categories are available
            var categories = entity.ProductCategories?.Select(pc => pc.Category!).ToList() ?? new List<Category>();

            return new Response<ProductViewModel?>
            {
                Data = ToViewModel(entity, images, categories),
                Status = true,
                Message = "Product"
            };
        }

        public async Task<Response<Guid>> CreateAsync(ProductCreateViewModel model, List<IFormFile> files)
        {
            //save images
            var result = await _fileService.SaveImagesAsync(
                files,
                MaxFileSizeBytes,
                AllowedExtensions,
                ProductsFolderName
                );
            //Some Images were invalid return error
            if (result.Status == false)
            {
                return new Response<Guid>
                {
                    Data = Guid.Empty,
                    Status = false,
                    Message = result.Message
                };
            }
            model.ImageUrls = result.Data;
            //create product entity
            var entity = new ProductEntity
            {
                Name = model.Name,
                Brand = model.Brand,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity
            };
            var created = await _repo.CreateAsync(entity);
            //create images
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
            //link categories
            if (model.CategoryIds.Any())
                await _repo.AddProductCategoriesAsync(created.Id, model.CategoryIds);

            return new Response<Guid>
            {
                Data = created.Id,
                Status = true,
                Message = "Product Created Successfully"
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

        private static ProductViewModel ToViewModel(ProductEntity p, List<ProductImage> images,List<Category> categories)
        {
            var rating = p.Reviews != null && p.Reviews.Any()
                ? p.Reviews.Average(r => r.Rating)
                : 0.0;

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

                OriginalPrice = CalculateOriginalPrice(p),
                Discount = p.DiscountPercentage.HasValue ? (int?)Math.Round(p.DiscountPercentage.Value) : null,

                Specifications = MapSpecifications(p),

                IsFlashDeal = CalculateIsFlashDeal(p),
                IsFeatured = IsFeatured(p),
                IsBestSeller = IsBestSeller(p),

                ImageUrl = mainImage,
                ImageUrls = images.Select(i => i.ImageUrl).ToList(),

                Categories = categories
                    .Select(c => new CategoryViewModel { Id = c.Id, Name = c.Name })
                    .ToList(),

                Rating = Math.Round(rating, 2),
                ReviewCount = p.Reviews?.Count ?? 0,
                Stock = p.StockQuantity
            };
        }


       

        private static decimal? CalculateOriginalPrice(ProductEntity p)
        {
            if (!p.DiscountPercentage.HasValue || p.DiscountPercentage.Value <= 0 || p.DiscountPercentage.Value >= 100)
                return null;

            var percentage = p.DiscountPercentage.Value / 100m;
            var denom = 1 - percentage;

            if (denom <= 0) return null;

            return Math.Round(p.Price / denom, 2);
        }

        private static bool CalculateIsFlashDeal(ProductEntity p)
        {
            if (!p.DiscountPercentage.HasValue || p.DiscountPercentage.Value <= 0)
                return false;

            var discount = p.DiscountPercentage.Value;

            bool largeDiscount = discount >= 30;
            bool recentProduct = (DateTime.UtcNow - p.CreatedAt).TotalDays <= 2;

            return largeDiscount || (recentProduct && discount >= 15);
        }

        private static bool IsFeatured(ProductEntity p)
        {
            // 1️⃣ New arrivals: created within last 14 days
            bool isNewArrival = (DateTime.UtcNow - p.CreatedAt).TotalDays <= 14;

            //Add any conditions for featured products here



            // If any condition is true, mark as featured
            return isNewArrival ;
        }


        private static bool IsBestSeller(ProductEntity p)
        {
            int totalSold = p.OrderItems?.Sum(oi => oi.Quantity) ?? 0;

            int recentSold = p.OrderItems?
                .Where(oi => oi.Order != null && (DateTime.UtcNow - oi.Order.OrderDate).TotalDays <= 30)
                .Sum(oi => oi.Quantity) ?? 0;

            return totalSold >= 20 || recentSold >= 10;
        }

        private static Dictionary<string, string> MapSpecifications(ProductEntity p)
        {
            if (p.ProductSpecifications == null)
                return new Dictionary<string, string>();

            return p.ProductSpecifications
                .Where(s => !string.IsNullOrEmpty(s.Key))
                .ToDictionary(s => s.Key, s => s.Value ?? string.Empty);
        }




    }
}
