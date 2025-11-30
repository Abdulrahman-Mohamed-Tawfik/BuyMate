using BuyMate.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.BLL.Contracts.Repositories
{
    public interface IOrderItemRepository : ICommonRepository<OrderItem>
    {
        Task<OrderItem?> GetOrderItemWithProductAsync(Guid itemId);
        Task<bool> DeleteOrderItemsByOrderIdAsync(Guid orderId);

    }
}
