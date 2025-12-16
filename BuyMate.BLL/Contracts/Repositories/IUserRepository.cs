using BuyMate.Model.Entities;

namespace BuyMate.BLL.Contracts.Repositories
{
    public interface IUserRepository : ICommonRepository<User>
    {
        public Task<IQueryable<User>> FilterAsync(Guid? id = null, bool? isDeleted = false);
        public Task<IQueryable<User>> SearchByNameAsync(string? Filter);
    }
}
