using BuyMate.DTO.Category; 

namespace BuyMate.BLL.Contracts
{
    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetAllAsync();
        Task<CategoryViewModel?> GetByIdAsync(Guid id); 
        Task<CategoryViewModel> CreateAsync(CreateCategoryDto dto);
        Task<bool> UpdateAsync(Guid id, CreateCategoryDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}