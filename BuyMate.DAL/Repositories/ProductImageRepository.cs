using BuyMate.BLL.Contracts.Repositories;
using BuyMate.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuyMate.DAL.Repositories
{
    public class ProductImageRepository : CommonRepository<ProductImage>, IProductImageRepository
    {
        private readonly BuyMateDbContext _context;

        public ProductImageRepository(BuyMateDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ProductImage>> GetByProductIdAsync(Guid productId)
        {
            return await _context.Set<ProductImage>()
                .Where(i => i.ProductId == productId)
                .ToListAsync();
        }

        public async Task RemoveByProductIdAsync(Guid productId)
        {
            var images = await _context.Set<ProductImage>()
                .Where(i => i.ProductId == productId)
                .ToListAsync();
            if (images.Count == 0) return;
            _context.Set<ProductImage>().RemoveRange(images);
            await _context.SaveChangesAsync();
        }

        public override IQueryable<ProductImage> OrderBy(IQueryable<ProductImage> entities, string? orderBy, bool isAccending = true)
        {
            return entities;
        }
    }
}