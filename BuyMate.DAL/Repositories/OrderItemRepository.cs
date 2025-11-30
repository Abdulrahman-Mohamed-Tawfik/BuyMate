using BuyMate.BLL.Contracts.Repositories;
using BuyMate.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.DAL.Repositories
{
    internal class OrderItemRepository : CommonRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(BuyMateDbContext context) : base(context)
        {
        }
        public Task<OrderItem?> GetCartItemWithProductAsync(Guid itemId)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<OrderItem> OrderBy(IQueryable<OrderItem> entities, string? orderBy, bool isAccending = true)
        {
            throw new NotImplementedException();
        }
    }
}
