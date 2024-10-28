using Microsoft.EntityFrameworkCore.Storage;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;

namespace WebApp.Repositories;

public interface IUnitOfWork
{
    IAppRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>;
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    void Dispose();
    ValueTask DisposeAsync();
}

public class UnitOfWork(AppDbContext context, IServiceProvider serviceProvider)
    : IDisposable, IAsyncDisposable, IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IAppRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
    {
        return serviceProvider.GetService<IAppRepository<TEntity, TKey>>()!;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            if (_transaction != null) await _transaction.CommitAsync();
            await DisposeAsync();
        }
        catch
        {
            await RollbackAsync();
            await DisposeAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null) await _transaction.RollbackAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null) await _transaction.DisposeAsync();
        await context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
