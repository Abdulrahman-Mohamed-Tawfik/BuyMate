using BuyMate.DTO.Category;
using BuyMate.DTO.Common;
using Microsoft.AspNetCore.Http;

namespace BuyMate.BLL.Contracts
{
    public interface ICategoryService
    {
        Task<Response<List<CategoryViewModel>>> GetAllAsync();
        Task<Response<CategoryViewModel>> GetByIdAsync(Guid id);
        Task<Response<CategoryViewModel>> CreateAsync(CreateCategoryDto dto, IFormFile? imageFile = null);
        Task<Response<bool>> UpdateAsync(Guid id, CreateCategoryDto dto, IFormFile? imageFile = null); // default param
        Task<Response<bool>> DeleteAsync(Guid id);
    }
}