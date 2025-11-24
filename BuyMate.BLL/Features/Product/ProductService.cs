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
            ProductFilter? filter = null)
        {

            filter ??= new ProductFilter();
            //Get All Products
            var query = _repo.GetAllQueryable();


            // apply additional filters
            query = ApplyFilters(query, filter);

            int totalCount = query.Count();


            //pagination
            var products = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

           

            // Include related data

            var ids = products.Select(p => p.Id).ToList();
            var allImages = await _imageRepo.GetAllQueryable(i => ids.Contains(i.ProductId)).ToListAsync();
            //Group images by product
            var imagesByProduct = allImages
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.ToList());
            //Map to ViewModel
            var ProductsList = products.Select(p =>
            {
                var imgs = imagesByProduct.TryGetValue(p.Id, out var gi) ? gi : new List<ProductImage>();
                var cats = p.ProductCategories.Select(pc => pc.Category!).ToList();
                return ToViewModel(p, imgs, cats);
            }).ToList();

            return new PaginatedResponse<List<ProductViewModel>>
            {
                Data = ProductsList,
                Message = "Products",
                Status = true,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
       
        private IQueryable<ProductEntity> ApplyFilters(IQueryable<ProductEntity> query, ProductFilter? filter)
        {
            if (filter == null) return query;
            //Filter by Category
            if (filter.CategoryId.HasValue)
                query = query.Where(p =>
                    p.ProductCategories.Any(pc => pc.CategoryId == filter.CategoryId.Value)
                );
            //Filter by Search
            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(p => p.Name.Contains(filter.Search));
            //Filter by Brand
            if (!string.IsNullOrWhiteSpace(filter.Brand))
            {
                var b = filter.Brand.Trim();
                query = query.Where(p => p.Brand != null && p.Brand.ToLower() == b.ToLower());
            }
            //Filter by Price Range
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            //Filter by Discount
            if (filter.HasDiscount.HasValue)
            {
                if (filter.HasDiscount.Value)
                    query = query.Where(p => p.DiscountPercentage.HasValue && p.DiscountPercentage.Value > 0);
                else
                    query = query.Where(p => !p.DiscountPercentage.HasValue || p.DiscountPercentage.Value <= 0);
            }
            //Filter by Featured
            if (filter.IsFeatured.HasValue)
            {
                var fourteenDaysAgo = DateTime.UtcNow.AddDays(-14);
                if (filter.IsFeatured.Value)
                    query = query.Where(p =>
                        // use the same logic as IsFeatured private method
                        p.CreatedAt >= fourteenDaysAgo
                    );
                else
                    query = query.Where(p => p.CreatedAt < fourteenDaysAgo);
            }
            // Apply Ordering
            query = _repo.OrderBy(query, filter.OrderBy, filter.Asc)
                        .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                        .Include(p => p.Reviews);

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

            // After creating 'created' entity and categories:
            if (model.Specifications != null && model.Specifications.Any())
            {
                var specs = model.Specifications.Select(s => new ProductSpecification
                {
                    Key = s.Key,
                    Value = s.Value,
                    ProductId = created.Id
                }).ToList();

                await _repo.AddProductSpecificationsAsync(created.Id, specs);
            }


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

            // update discount if provided
            if (model.DiscountPercentage.HasValue)
            {
                entity.DiscountPercentage = model.DiscountPercentage.Value;
            }
           

            await _repo.UpdateAsync(entity);

            // If ImageUrls is provided (even empty), user intends to control images: remove and recreate
            if (model.ImageUrls != null && model.ImageUrls.Count != 0)
            {
                // fetch existing images so we can delete files that are removed
                var existingImages = await _imageRepo.GetByProductIdAsync(id);

                // determine which urls were removed by the user
                var kept = new HashSet<string>(model.ImageUrls ?? new List<string>());
                var toDelete = existingImages
                    .Select(i => i.ImageUrl)
                    .Where(url => !kept.Contains(url))
                    .ToList();

                // remove DB records
                await _imageRepo.RemoveByProductIdAsync(id);

                // delete physical files for removed images (best-effort)
                foreach (var url in toDelete)
                {
                    try
                    {
                        _fileService.DeleteImage(url);
                    }
                    catch
                    {
                        // ignore file deletion errors to not fail the update
                    }
                }

                if (model.ImageUrls.Any())
                {
                    var imgs = model.ImageUrls.Select((url, index) => new ProductImage
                    {
                        ProductId = id,
                        ImageUrl = url,
                        IsMain = index == 0
                    }).ToList();

                    await _imageRepo.CreateRangeAsync(imgs);
                }
            }

            // update categories
            await _repo.RemoveProductCategoriesAsync(id);

            if (model.CategoryIds.Any())
                await _repo.AddProductCategoriesAsync(id, model.CategoryIds);

            // update specifications: remove old ones and add new
            await _repo.RemoveProductSpecificationsAsync(id);
            if (model.Specifications != null && model.Specifications.Any())
            {
                var specs = model.Specifications.Select(s => new ProductSpecification
                {
                    Key = s.Key,
                    Value = s.Value,
                    ProductId = id
                }).ToList();
                await _repo.AddProductSpecificationsAsync(id, specs);
            }

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


        Task<List<string>> IProductService.GetAllBrandsAsync()
        {
            return _repo.GetAllBrandsAsync();
        }

        Task<List<CategoryViewModel>> IProductService.GetAllCategoriesAsync()
        {
            return _repo.GetAllCategoriesAsync();
        }

        // Helper Methods
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
            return isNewArrival;
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
