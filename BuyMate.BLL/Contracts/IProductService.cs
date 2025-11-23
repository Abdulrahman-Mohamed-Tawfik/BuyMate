using System;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BuyMate.BLL.Contracts
{
    public interface IProductService
    {
        Task<Response<List<ProductViewModel>>> GetAllAsync(
            string? search = null,
            string? orderBy = null,
            bool asc = true,
            Guid? categoryId = null,
            string? brand = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? hasDiscount = null,
            bool? isFeatured = null
        );
        Task<Response<ProductViewModel?>> GetByIdAsync(Guid id);
        Task<Response<Guid>> CreateAsync(ProductCreateViewModel model, List<IFormFile> files);
        Task<Response<bool>> UpdateAsync(Guid id, ProductUpdateViewModel model);
        Task<Response<bool>> DeleteAsync(Guid id);

        // NEW: Pagination with filters
        Task<PaginatedResponse<List<ProductViewModel>>> GetAllPaginatedAsync(
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
            bool? isFeatured = null
        );
    }
}
