using BuyMate.BLL.Contracts.Repositories;
using BuyMate.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.DAL.Repositories
{
    public class OrderRepository : CommonRepository<Order>, IOrderRepository
    {

        public OrderRepository(BuyMateDbContext context) : base(context)
        {
        }
        public Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Order?> GetOrderAsync(string orderId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Order?> GetOrderWithItemsAsync(string orderId)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<Order> OrderBy(IQueryable<Order> entities, string? orderBy, bool isAccending = true)
        {
            throw new NotImplementedException();
        }
    }
}
