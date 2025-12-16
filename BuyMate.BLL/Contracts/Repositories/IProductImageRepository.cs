using BuyMate.Model.Entities;

namespace BuyMate.BLL.Contracts.Repositories
{
    public interface IProductImageRepository : ICommonRepository<ProductImage>
    {
        Task<List<ProductImage>> GetByProductIdAsync(Guid productId);
        Task RemoveByProductIdAsync(Guid productId);
    }
}