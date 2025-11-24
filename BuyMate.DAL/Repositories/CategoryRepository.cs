using BuyMate.BLL.Contracts.Repositories;
using BuyMate.Model.Entities;

namespace BuyMate.DAL.Repositories
{
    public class CategoryRepository : CommonRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(BuyMateDbContext context) : base(context)
        {
        }

        public override IQueryable<Category> OrderBy(IQueryable<Category> entities, string? orderBy, bool isAccending = true)
        {
            if (string.IsNullOrEmpty(orderBy))
            {
                return isAccending ? entities.OrderBy(c => c.Name) : entities.OrderByDescending(c => c.Name);
            }

            return isAccending ? entities.OrderBy(c => c.Name) : entities.OrderByDescending(c => c.Name);
        }
    }
}