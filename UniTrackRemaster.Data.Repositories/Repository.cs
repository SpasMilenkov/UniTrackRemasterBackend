using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;

namespace UniTrackRemaster.Data.Repositories;

/// <summary>
/// Generic repository implementation for common entity operations
/// </summary>
public class Repository<T>(UniTrackDbContext context) : IRepository<T> where T : class
{
    protected readonly UniTrackDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {

        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        // EntityFramework tracks changes, but we explicitly set the state to ensure the update
        _context.Entry(entity).State = EntityState.Modified;

        // Note: No direct async operation for Update, but returning Task for consistency
        return await Task.FromResult(entity);
    }

    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _dbSet.Remove(entity);
        return true;
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public virtual async Task<bool> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        return await Task.FromResult(true);
    }
}