using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate, string? includeProperties = null);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Guid id);
}

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Routine> Routines { get; }
    IRepository<RoutineLog> RoutineLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
} 