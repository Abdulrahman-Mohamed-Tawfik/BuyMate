using BuyMate.BLL.Contracts.Repositories;
using BuyMate.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuyMate.DAL.Repositories
{
    public class ProductRepository : CommonRepository<Product>, IProductRepository
    {
        private readonly BuyMateDbContext _context;

        public ProductRepository(BuyMateDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Reviews)
                .Include(p => p.Images)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IQueryable<Product>> SearchAsync(string? filter)
        {
            var query = await GetAsync();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.Trim().ToLower();

                query = query.Where(p =>
                    p.Name.ToLower().Contains(filter) ||
                    p.Description.ToLower().Contains(filter) ||
                    p.Price.ToString().Contains(filter) ||
                    p.StockQuantity.ToString().Contains(filter) ||
                    p.Images.Any(img => img.ImageUrl.ToLower().Contains(filter))
                );
            }

            return query;
        }

        public Task<IQueryable<Product>> GetAllWithCategoriesAsync()
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category);

            return Task.FromResult(query);
        }

        public Task<IQueryable<Product>> FilterByCategoryAsync(Guid categoryId)
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId));

            return Task.FromResult(query);
        }

        public async Task AddProductCategoriesAsync(Guid productId, List<Guid> categoryIds)
        {
            if (categoryIds == null || categoryIds.Count == 0) return;

            var links = categoryIds
                .Distinct()
                .Select(cid => new ProductCategory
                {
                    ProductId = productId,
                    CategoryId = cid
                })
                .ToList();

            await _context.ProductCategories.AddRangeAsync(links);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveProductCategoriesAsync(Guid productId)
        {
            var oldLinks = _context.ProductCategories
                .Where(pc => pc.ProductId == productId);

            _context.ProductCategories.RemoveRange(oldLinks);
            await _context.SaveChangesAsync();
        }

        public override IQueryable<Product> OrderBy(IQueryable<Product> entities, string? orderBy, bool isAccending = true)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
                return entities;

            return orderBy.ToLower() switch
            {
                "name"  => isAccending ? entities.OrderBy(p => p.Name)  : entities.OrderByDescending(p => p.Name),
                "price" => isAccending ? entities.OrderBy(p => p.Price) : entities.OrderByDescending(p => p.Price),
                _ => entities
            };
        }
    }
}
