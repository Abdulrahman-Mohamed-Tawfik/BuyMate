using BuyMate.Model.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuyMate.BLL.Contracts.Repositories
{
    public interface IOrderRepository : ICommonRepository<Order>
    {
        Task<Order?> GetOrderAsync(Guid orderId);
        Task<Order?> GetOrderWithItemsAsync(Guid orderId);


        Task<List<Order>> GetOrdersByUserIdAsync(string userId);

        Task<List<Order>> GetAllOrdersWithItemsAsync();

    }
}
