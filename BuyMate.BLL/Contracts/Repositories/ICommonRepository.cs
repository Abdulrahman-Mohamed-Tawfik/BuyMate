using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace BuyMate.BLL.Contracts.Repositories
{
    public interface ICommonRepository<TEntity>
    {
        Task<IQueryable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null);
        Task<IQueryable<TEntity>> GetAsync<TProperty>(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>>? include = null);
        Task<IQueryable<TEntity>> GetAllAsync();
        Task CreateRangeAsync(IEnumerable<TEntity> tutorialSteps);
        Task<TEntity> CreateAsync(TEntity entity);
        TEntity Create(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        IQueryable<TEntity> OrderBy(IQueryable<TEntity> entities, string? orderBy, bool isAccending = true);
        Task<bool> DeletePhysicallyAsync(Guid id);
        bool DeletePhysically(Guid id);
        Task<bool> DeleteSoftAsync(TEntity entity);
        Task<int> SaveChangesAsync();
        int SaveChanges();
        List<TEntity> Get(Expression<Func<TEntity, bool>>? filter = null);
        IQueryable<TEntity> GetAllQueryable(Expression<Func<TEntity, bool>>? filter = null);
        IQueryable<TEntity> GetAllQueryableInclude(Expression<Func<TEntity, bool>>? filter = null, params Expression<Func<TEntity, object>>[] includes);
        List<TEntity> GetAll();
        Task RemoveRangeAsync(IEnumerable<TEntity> entities);

    }
}
