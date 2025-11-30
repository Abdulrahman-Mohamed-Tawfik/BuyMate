using BuyMate.BLL.Contracts.Repositories;
using BuyMate.Model.Entities;
using Microsoft.EntityFrameworkCore;
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
        public async Task<List<Order>> GetAllOrdersWithItemsAsync()
        {
            var query = await GetAsync(null, q => q
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images)
                );
            return query.ToList();
        }

        public async Task<Order?> GetOrderAsync(Guid orderId)
        {
            var query = await GetAsync(o => o.Id == orderId);
            var order = query.SingleOrDefault();
            return order;
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                return new List<Order>(); // Change from Enumerable.Empty<Order>() to new List<Order>()

            var query = await GetAsync(o => o.UserId == userGuid, q => q
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images)
                );

            return query.ToList();
        }

        public async Task<Order?> GetOrderWithItemsAsync(Guid orderId)
        {
            var query = await GetAsync(
                o => o.Id == orderId, q => q
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images)

                );
              

            var order = query.SingleOrDefault();
            return order;
        }

        public override IQueryable<Order> OrderBy(IQueryable<Order> entities, string? orderBy, bool isAccending = true)
        {
            throw new NotImplementedException();
        }
    }
}
