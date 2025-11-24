using System;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BuyMate.BLL.Contracts
{
    public interface IProductService
    {
     
        Task<Response<ProductViewModel?>> GetByIdAsync(Guid id);
        Task<Response<Guid>> CreateAsync(ProductCreateViewModel model, List<IFormFile> files);
        Task<Response<bool>> UpdateAsync(Guid id, ProductUpdateViewModel model);
        Task<Response<bool>> DeleteAsync(Guid id);

        // NEW: Pagination with filters
        Task<PaginatedResponse<List<ProductViewModel>>> GetAllPaginatedAsync(
            ProductFilter? filter = null
        );

        Task<List<string>> GetAllBrandsAsync();
    }
}
