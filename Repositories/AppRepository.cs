using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;

namespace WebApp.Repositories
{
    public interface IAppRepository<T, in TK> where T : BaseEntity
    {
        Task<T> CreateAsync(T entity);
        Task CreateManyAsync(IEnumerable<T> entities);
        IQueryable<T> Find(Expression<Func<T, bool>> predicate, params string[] props);
        Task<List<T>> FindAllAsync(int skip, int take, string? sortProperty);
        Task<T?> FindByIdAsync(TK id);
        Task UpdateAsync(T entity);
        Task<bool> ExistAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        Task<bool> SoftDelete(TK id);
    }

    public class AppRepository<T, TK> : IAppRepository<T, TK> where T : BaseEntity
    {
        private readonly AppDbContext _db;
        private readonly DbSet<T> _dbSet;

        public AppRepository(AppDbContext dbContext)
        {
            _db = dbContext;
            _dbSet = _db.Set<T>();
        }

        public async Task<List<T>> FindAllAsync(int skip, int take, string? sortProperty)
        {
            sortProperty ??= "Id ascending";
            return await _dbSet
                .Where(t => t.Deleted == false)
                .OrderBy(sortProperty)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<T> CreateAsync(T entity)
        {
            var saved = (await _dbSet.AddAsync(entity)).Entity;
            await _db.SaveChangesAsync();
            return saved;
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> predicate, params string[] props)
        {
            var query = _dbSet.Where(predicate);
            return props.IsNullOrEmpty()
                ? query
                : props.Aggregate(query, (current, prop) => current.Include(prop));
        }

        public async Task CreateManyAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _db.SaveChangesAsync();
        }

        public async Task<T?> FindByIdAsync(TK id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistAsync(Expression<Func<T, bool>> predicate)
        {
            var query = _dbSet.Where(predicate);
            return await query.AnyAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<bool> SoftDelete(TK id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity is null) return false;
            entity.Deleted = true;
            _dbSet.Update(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}