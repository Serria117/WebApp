﻿using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver.Linq;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using Z.Expressions.Compiler;

namespace WebApp.Repositories;

public interface IAppRepository<T, in TK> where T : BaseEntity<TK>
{
    Task<T> CreateAsync(T entity);
    Task CreateManyAsync(IEnumerable<T> entities);
    IQueryable<T> Find(Expression<Func<T, bool>> condition, params string[] include);

    IQueryable<T> Find(Expression<Func<T, bool>> condition, string? sortBy = "Id", string? order = SortOrder.ASC,
                       params string[] include);

    Task<T?> FindByIdAsync(TK id);
    Task<T> UpdateAsync(T entity);
    Task<bool> ExistAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(IQueryable<T> query);
    Task<bool> SoftDeleteAsync(TK id);
    IQueryable<T> GetQueryable();
    Task<int> CountAsync();
    Task<bool> SoftDeleteManyAsync(params TK[] ids);
    IQueryable<T> FindAndSort(Expression<Func<T, bool>> condition, string[] props, string[] sortStrings);
    T Attach(TK id);
}

public class AppRepository<T, TK> : IAppRepository<T, TK> where T : BaseEntity<TK>, new()
{
    private readonly AppDbContext _db;
    private readonly DbSet<T> _dbSet;

    public AppRepository(AppDbContext dbContext)
    {
        _db = dbContext;
        _dbSet = _db.Set<T>();
    }

    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public T Attach(TK id)
    {
        return _dbSet.Attach(new T {Id = id}).Entity;
    }

    public async Task<List<T>> FindAllAsync(int skip, int take, string? sortProperty, string? include = null)
    {
        sortProperty ??= "Id ASC";
        var query = _dbSet.Where(t => !t.Deleted);
        if (include != null)
        {
            query = query.Include(include);
        }

        return await query.OrderBy(sortProperty)
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

    public IQueryable<T> Find(Expression<Func<T, bool>> condition, params string[] include)
    {
        var query = _dbSet.Where(condition);
        if (!include.IsNullOrEmpty())
        {
            query = include.Aggregate(query, (current, prop) => current.Include(prop));
        }

        return query.OrderBy("Id DESC");
    }

    public IQueryable<T> Find(Expression<Func<T, bool>> condition, string? sortBy = "Id", string? order = "DESC",
                              params string[] include)
    {
        var query = _dbSet.Where(condition);
        if (!include.IsNullOrEmpty())
        {
            query = include.Aggregate(query, (current, prop) => current.Include(prop));
        }

        return query.OrderBy($"{sortBy} {order}");
    }

    public IQueryable<T> FindAndSort(Expression<Func<T, bool>> condition, string[] props, string[] sortStrings)
    {
        var query = _dbSet.Where(condition);
        if (!props.IsNullOrEmpty())
        {
            query = props.Aggregate(query, (current, prop) => current.Include(prop));
        }

        if (!sortStrings.IsNullOrEmpty())
        {
            query = sortStrings.Aggregate(query, (current, sort) => current.OrderBy(sort));
        }

        return query;
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

    public async Task<T> UpdateAsync(T entity)
    {
        var res = _dbSet.Update(entity);
        await _db.SaveChangesAsync();
        return res.Entity;
    }

    public async Task<bool> ExistAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).AsNoTracking().AnyAsync();
    }

    public async Task<int> CountAsync(IQueryable<T> query)
    {
        return await query.CountAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<bool> SoftDeleteAsync(TK id)
    {
        var result = await Find(x => x.Id != null && x.Id.Equals(id) && !x.Deleted)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Deleted, true));
        return result > 0;
    }

    public async Task<bool> SoftDeleteManyAsync(params TK[] ids)
    {
        var result = await Find(t => ids.Contains(t.Id) && !t.Deleted)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Deleted, true));
        return result > 0;
    }
}