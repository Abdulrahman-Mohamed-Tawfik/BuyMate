using BuyMate.Model.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuyMate.BLL.Contracts.Repositories
{
    public interface IOrderRepository : ICommonRepository<Order>
    {
        Task<Order?> GetOrderAsync(string orderId);
        Task<Order?> GetOrderWithItemsAsync(string orderId);


        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);

        Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync();

    }
}
