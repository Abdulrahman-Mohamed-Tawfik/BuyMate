using BuyMate.Model.Entities;

namespace BuyMate.BLL.Contracts.Repositories;

public interface ICartRepository : ICommonRepository<Cart>
{
    Task<Cart?> GetCartAsync(string userId);
    Task<Cart?> GetCartWithItemsAsync(string userId);
}
