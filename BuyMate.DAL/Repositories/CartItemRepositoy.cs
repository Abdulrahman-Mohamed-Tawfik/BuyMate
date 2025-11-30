using BuyMate.BLL.Contracts.Repositories;
using BuyMate.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuyMate.DAL.Repositories;

public class CartItemRepositoy : CommonRepository<CartItem>, ICartItemRepository
{
    public CartItemRepositoy(BuyMateDbContext context) : base(context)
    {
    }

    public async Task<CartItem?> GetCartItemWithProductAsync(Guid itemId)
    {
        var query = await GetAsync(ci => ci.Id == itemId, q => q.Include(c => c.Product));
        var cartItem = query.SingleOrDefault();
        return cartItem;
    }

    public async Task<bool> DeleteCartItemsByCartIdAsync(Guid cartId)
    {
        var items = (await GetAsync(ci => ci.CartId == cartId)).ToList();
        if (!items.Any()) return true;

        _context.Set<CartItem>().RemoveRange(items);
        await _context.SaveChangesAsync();
        return true;
    }

    public override IQueryable<CartItem> OrderBy(IQueryable<CartItem> entities, string? orderBy, bool isAccending = true)
    {
        return entities;
    }
}

