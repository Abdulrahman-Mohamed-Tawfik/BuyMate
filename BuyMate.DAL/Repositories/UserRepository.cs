using BuyMate.BLL.Contracts;
using BuyMate.Model.Entities;

namespace BuyMate.DAL.Repositories
{
    public class UserRepository : CommonRepository<User>, IUserRepository
    {
        public UserRepository(BuyMateDbContext context) : base(context)
        {
        }

        public async Task<IQueryable<User>> FilterAsync(Guid? id = null, bool? isDeleted = false)
        {
            return (await GetAsync()).Where(a => id == null || a.Id == id);
        }

        public override IQueryable<User> OrderBy(IQueryable<User> entities, string? orderBy, bool isAccending = true)
        {

            return entities;
        }

        public async Task<IQueryable<User>> SearchByNameAsync(string? Filter)
        {
            return (await GetAsync()).Where(a => Filter == null || a.FirstName.Contains(Filter) || a.LastName.Contains(Filter));
        }
    }
}
