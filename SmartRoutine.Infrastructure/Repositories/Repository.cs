using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartRoutine.Infrastructure.Data;

namespace SmartRoutine.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = _dbSet.AsQueryable();
        var isDeletedProp = typeof(T).GetProperty("IsDeleted");
        if (isDeletedProp != null)
        {
            // Sadece silinmemişleri getir
            query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
        }
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null)
    {
        var query = _dbSet.Where(predicate);
        
        // Include properties if specified
        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
        }
        
        var isDeletedProp = typeof(T).GetProperty("IsDeleted");
        if (isDeletedProp != null)
        {
            query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
        }
        return await query.ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        var addedEntity = await _dbSet.AddAsync(entity);
        return addedEntity.Entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
        return entity;
    }

    public async Task DeleteAsync(T entity)
    {
        var isDeletedProp = typeof(T).GetProperty("IsDeleted");
        var deletedAtProp = typeof(T).GetProperty("DeletedAt");
        if (isDeletedProp != null && deletedAtProp != null)
        {
            isDeletedProp.SetValue(entity, true);
            deletedAtProp.SetValue(entity, DateTime.UtcNow);
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
} 