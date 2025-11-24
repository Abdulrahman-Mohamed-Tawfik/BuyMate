using BuyMate.BLL.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BuyMate.DAL.Repositories
{
    public abstract class CommonRepository<TEntity> : ICommonRepository<TEntity> where TEntity : class
    {
        private readonly BuyMateDbContext _context;
        public CommonRepository(BuyMateDbContext context)
        {
            _context = context;
        }
        public virtual async Task<IQueryable<TEntity>> GetAllAsync()
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            return await Task.FromResult(query);
        }
        public virtual async Task<IQueryable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await Task.FromResult(query);
        }
        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
            await _context.SaveChangesAsync();
            _context.Entry(entity).Reload();
            return entity;
        }
        public virtual TEntity Create(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            _context.SaveChanges();
            _context.Entry(entity).Reload();
            return entity;
        }
        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public virtual async Task<List<TEntity>> CreateAsync(List<TEntity> entities)
        {
            if (entities == null || entities.Count == 0)
                throw new ArgumentException("The entity list cannot be null or empty.", nameof(entities));

            await _context.Set<TEntity>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }
        public virtual async Task<bool> DeletePhysicallyAsync(Guid id)
        {
            var entity = await _context.Set<TEntity>().FindAsync(id);
            if (entity is null)
            {
                return false;
            }
            else
            {
                _context.Set<TEntity>().Remove(entity);
                return await _context.SaveChangesAsync() > 0;
            }

        }
        public virtual async Task<bool> DeletePhysicallyAsync(long id)
        {
            var entity = await _context.Set<TEntity>().FindAsync(id);
            if (entity is null)
            {
                return false;
            }
            else
            {
                _context.Set<TEntity>().Remove(entity);
                return await _context.SaveChangesAsync() > 0;
            }

        }
        public bool DeletePhysically(Guid id)
        {
            var entity = _context.Set<TEntity>().Find(id);
            if (entity == null)
            {
                return false;
            }
            else
            {
                _context.Set<TEntity>().Remove(entity);
                return _context.SaveChanges() > 0;
            }

        }
        public abstract IQueryable<TEntity> OrderBy(IQueryable<TEntity> entities, string? orderBy, bool isAccending = true);
        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public virtual int SaveChanges()
        {
            return _context.SaveChanges();
        }
        public async Task CreateRangeAsync(IEnumerable<TEntity> tutorialSteps)
        {
            // Add all tutorial steps to the context
            await _context.Set<TEntity>().AddRangeAsync(tutorialSteps);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }
        public List<TEntity> GetAll()
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            return query.ToList();
        }
        public List<TEntity> Get(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return query.ToList();
        }
        public virtual IQueryable<TEntity> GetAllQueryable(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return query;
        }
        public virtual IQueryable<TEntity> GetAllQueryableInclude(Expression<Func<TEntity, bool>>? filter = null, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includes?.Any() == true)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // Apply filter if provided
            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }
        public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }
    }
}
