using BuyMate.BLL.Contracts.Repositories;
using BuyMate.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuyMate.DAL.Repositories;

public class CartRepository : CommonRepository<Cart>, ICartRepository
{
    public CartRepository(BuyMateDbContext context) : base(context)
    {
    }

    public async Task<Cart?> GetCartAsync(string userId)
    {
        var query = await GetAsync(c => c.UserId.ToString() == userId);
        var cart = query.SingleOrDefault();
        return cart;
    }

    public async Task<Cart?> GetCartWithItemsAsync(string userId)
    {
        var query = await GetAsync(
            c => c.UserId.ToString() == userId, q => q
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .ThenInclude(p => p.ProductImages)
            );
        var cart = query.SingleOrDefault();
        return cart;
    }

    public override IQueryable<Cart> OrderBy(IQueryable<Cart> entities, string? orderBy, bool isAccending = true)
    {
        throw new NotImplementedException();
    }

}

