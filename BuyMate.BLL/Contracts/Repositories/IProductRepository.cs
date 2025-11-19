using BuyMate.Model.Entities;

namespace BuyMate.BLL.Contracts.Repositories
{
    public interface IProductRepository : ICommonRepository<Product>
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<bool> SoftDeleteAsync(Guid id);
        Task<IQueryable<Product>> SearchAsync(string? filter);

        Task<IQueryable<Product>> GetAllWithCategoriesAsync();
        Task<IQueryable<Product>> FilterByCategoryAsync(Guid categoryId);

        Task AddProductCategoriesAsync(Guid productId, List<Guid> categoryIds);
        Task RemoveProductCategoriesAsync(Guid productId);
    }
}
