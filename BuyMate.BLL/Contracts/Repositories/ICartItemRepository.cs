using BuyMate.Model.Entities;

namespace BuyMate.BLL.Contracts.Repositories;

public interface ICartItemRepository : ICommonRepository<CartItem>
{
    Task<CartItem?> GetCartItemWithProductAsync(Guid itemId);
}
