using BuyMate.BLL.Constants;
using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Helpers;
using BuyMate.BLL.Contracts.Repositories;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.Category;
using BuyMate.DTO.ViewModels.Product;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

// ⭐ مهم جداً
using ProductEntity = BuyMate.Model.Entities.Product;

namespace BuyMate.BLL.Features.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IFileService _fileService;

        public ProductService(IProductRepository repo, IProductImageRepository imageRepo, IFileService fileService)
        {
            _productRepository = repo;
            _productImageRepository = imageRepo;
            _fileService = fileService;
        }

        public async Task<PaginatedResponse<List<ProductViewModel>>> GetAllPaginatedAsync(ProductFilter? filter = null)
        {
            filter ??= new ProductFilter();
            // Get All Products
            var query = _productRepository.GetAllQueryable();

            // apply additional filters
            query = ApplyFilters(query, filter);

            int totalCount = query.Count();

            // pagination
            var products = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Include related data
            var ids = products.Select(p => p.Id).ToList();
            var allImages = await _productImageRepository.GetAllQueryable(i => ids.Contains(i.ProductId)).ToListAsync();
            //Group images by product
            var imagesByProduct = allImages
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.ToList());
            //Map to ViewModel
            var productsList = products.Select(p =>
            {
                var imgs = imagesByProduct.TryGetValue(p.Id, out var gi) ? gi : new List<ProductImage>();
                var cats = p.ProductCategories.Select(pc => pc.Category!).ToList();
                return ToViewModel(p, imgs, cats);
            }).ToList();

            return PaginatedResponse<List<ProductViewModel>>.Success(productsList, totalCount, filter.PageSize, filter.PageNumber, "Products Retrieved Successfully");
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
            // Use the effective price (discounted price when discount applies) for filtering
            if (filter.MinPrice.HasValue)
            {
                var min = filter.MinPrice.Value;
                query = query.Where(p =>
                ((
                p.DiscountPercentage.HasValue && p.DiscountPercentage.Value > 0 && p.DiscountPercentage.Value < 100)
                   ? p.Price * (1 - p.DiscountPercentage.Value / 100m)
                    : p.Price) >= min
                );
            }
            if (filter.MaxPrice.HasValue)
            {
                var max = filter.MaxPrice.Value;
                query = query.Where(p =>
                 ((p.DiscountPercentage.HasValue && p.DiscountPercentage.Value > 0 && p.DiscountPercentage.Value < 100)
                 ? p.Price * (1 - p.DiscountPercentage.Value / 100m)
                 : p.Price) <= max
                 );
            }
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
            query = _productRepository.OrderBy(query, filter.OrderBy, filter.Asc)
                        .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                        .Include(p => p.Reviews);

            return query;
        }

        public async Task<Response<ProductViewModel?>> GetByIdAsync(Guid id)
        {
            var entity = await _productRepository.GetByIdAsync(id);
            if (entity == null)
                return Response<ProductViewModel?>.Fail("Not Found");

            // Load related data that may not have been included by repository
            var images = await _productImageRepository.GetByProductIdAsync(id);

            // Ensure categories are available
            var categories = entity.ProductCategories?.Select(pc => pc.Category!).ToList() ?? new List<Category>();

            return Response<ProductViewModel?>.Success(
                ToViewModel(entity, images, categories),
                "Product Retrieved Successfully"
            );
        }

        public async Task<Response<Guid>> CreateAsync(ProductCreateViewModel model, List<IFormFile> files)
        {
            //save images
            var result = await _fileService.SaveImagesAsync(
                files,
                AppConstants.MaxImageFileSizeBytes,
                AppConstants.AllowedImageExtensions,
                AppConstants.ProductsFolderName
            );
            //Some Images were invalid return error
            if (result.Status == false)
            {
                return Response<Guid>.Fail(result.Message ?? "Failed to upload images.");
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
            var createdProduct = await _productRepository.CreateAsync(entity);
            //create images
            if (model.ImageUrls.Any())
            {
                var images = model.ImageUrls.Select((url, index) => new ProductImage
                {
                    ProductId = createdProduct.Id,
                    ImageUrl = url,
                    IsMain = index == 0
                }).ToList();

                await _productImageRepository.CreateRangeAsync(images);
            }
            //link categories
            if (model.CategoryIds.Any())
                await _productRepository.AddProductCategoriesAsync(createdProduct.Id, model.CategoryIds);

            // After creating 'created' entity and categories:
            if (model.Specifications != null && model.Specifications.Any())
            {
                var specs = model.Specifications.Select(s => new ProductSpecification
                {
                    Key = s.Key,
                    Value = s.Value,
                    ProductId = createdProduct.Id
                }).ToList();

                await _productRepository.AddProductSpecificationsAsync(createdProduct.Id, specs);
            }

            return Response<Guid>.Success(createdProduct.Id, "Product Created Successfully");
        }

        public async Task<Response<bool>> UpdateAsync(Guid id, ProductUpdateViewModel model, List<IFormFile>? files)
        {

            var entity = await _productRepository.GetByIdAsync(id);
            if (entity == null)
                return new Response<bool> { Status = false, Data = false, Message = "Not Found" };

            // If there are uploaded files, save them and append returned URLs to model.ImageUrls
            if (files != null && files.Any())
            {
                var saveResult = await _fileService.SaveImagesAsync(
                    files,
                    AppConstants.MaxImageFileSizeBytes,
                    AppConstants.AllowedImageExtensions,
                    AppConstants.ProductsFolderName
                );

                if (!saveResult.Status)
                {
                    return Response<bool>.Fail(saveResult.Message);
                }

                // Append saved URLs to any existing ImageUrls that were preserved on the client
                model.ImageUrls ??= new List<string>();
                model.ImageUrls.AddRange(saveResult.Data);
            }

            entity.Name = model.Name;
            entity.Brand = model.Brand;
            entity.Description = model.Description;
            entity.Price = model.Price;
            entity.StockQuantity = model.StockQuantity;
            entity.UpdatedAt = DateTime.UtcNow;



            // update discount if provided
            if (model.DiscountPercentage.HasValue && model.DiscountPercentage > 0)
            {
                entity.DiscountPercentage = model.DiscountPercentage.Value;
            }
            else
            {
                entity.DiscountPercentage = null;
            }


            await _productRepository.UpdateAsync(entity);

            // If ImageUrls is provided (even empty), user intends to control images: remove and recreate
            if (model.ImageUrls != null && model.ImageUrls.Count != 0)
            {
                // fetch existing images so we can delete files that are removed
                var existingImages = await _productImageRepository.GetByProductIdAsync(id);

                // determine which urls were removed by the user
                var kept = new HashSet<string>(model.ImageUrls ?? new List<string>());
                var toDelete = existingImages
                    .Select(i => i.ImageUrl)
                    .Where(url => !kept.Contains(url))
                    .ToList();

                // remove DB records
                await _productImageRepository.RemoveByProductIdAsync(id);

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

                    await _productImageRepository.CreateRangeAsync(imgs);
                }
            }

            // update categories
            await _productRepository.RemoveProductCategoriesAsync(id);

            if (model.CategoryIds.Any())
                await _productRepository.AddProductCategoriesAsync(id, model.CategoryIds);

            // update specifications: remove old ones and add new
            await _productRepository.RemoveProductSpecificationsAsync(id);
            if (model.Specifications != null && model.Specifications.Any())
            {
                var specs = model.Specifications.Select(s => new ProductSpecification
                {
                    Key = s.Key,
                    Value = s.Value,
                    ProductId = id
                }).ToList();
                await _productRepository.AddProductSpecificationsAsync(id, specs);
            }

            return Response<bool>.Success(true, "Updated Successfully");
        }

        public async Task<Response<bool>> UpdateProductsStockAsync(List<ProductViewModel> products)
        {
            if (products is null || products.Count == 0)
            {
                return Response<bool>.Fail("Products Not Found ");
            }

            foreach (var product in products)
            {
                var entity = await _productRepository.GetByIdAsync(product.Id);

                entity.StockQuantity = product.StockQuantity;

                await _productRepository.UpdateAsync(entity);

            }

            return Response<bool>.Success(true, "Stock Updated Successfully");
        }
        public async Task<Response<bool>> DeleteAsync(Guid id)
        {
            var deleted = await _productRepository.SoftDeleteAsync(id);

            if (!deleted)
                return Response<bool>.Fail("Not Found");
            return Response<bool>.Success(true, "Deleted Successfully");
        }

        public async Task<Response<List<ProductViewModel>>> GetProductsByIdsAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return Response<List<ProductViewModel>>.Fail("No product IDs provided.");
            }

            // Remove duplicates just in case
            ids = ids.Distinct().ToList();

            // 1. Get the products
            var products = await _productRepository.GetAllQueryable(p => ids.Contains(p.Id))
                .ToListAsync();

            if (!products.Any())
            {
                return Response<List<ProductViewModel>>.Fail("No products found.");
            }

            // 3. Map to view models
            var result = products.Select(p =>
            {
                var imgs = new List<ProductImage>();

                var cats = new List<Category>();

                return ToViewModel(p, imgs, cats);
            }).ToList();

            return Response<List<ProductViewModel>>.Success(result, "Products loaded successfully.");
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
                Price = CalculateDiscountPrice(p),
                Brand = p.Brand,

                OriginalPrice = CalculateDiscountPrice(p) != p.Price ? p.Price : null,
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
                StockQuantity = p.StockQuantity
            };
        }

        Task<List<string>> IProductService.GetAllBrandsAsync()
        {
            return _productRepository.GetAllBrandsAsync();
        }

        // Helper Methods
        private static decimal CalculateDiscountPrice(ProductEntity p)
        {
            if (!p.DiscountPercentage.HasValue || p.DiscountPercentage.Value <= 0 || p.DiscountPercentage.Value >= 100)
                return p.Price;

            var percentage = p.DiscountPercentage.Value / 100m;
            var denom = 1 - percentage;

            if (denom <= 0) return p.Price;

            return Math.Round(p.Price * denom, 2);
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
