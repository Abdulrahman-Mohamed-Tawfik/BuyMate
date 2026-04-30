using BuyMate.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BuyMate.BLL.Contracts.Repositories;
namespace BuyMate.DAL.Repositories
{
    public class WishlistRepository : CommonRepository<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(BuyMateDbContext context) : base(context)
        {
        }

        public IQueryable<Wishlist> QueryWithDetails()
        {
            return _context.Wishlists
                .Include(w => w.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images);
        }

        //TODO: Deal with this method
        public override IQueryable<Wishlist> OrderBy(IQueryable<Wishlist> entities, string? orderBy, bool isAccending = true)
        {
            throw new NotImplementedException();
        }
    }

}
