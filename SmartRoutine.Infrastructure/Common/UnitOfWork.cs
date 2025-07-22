using Microsoft.EntityFrameworkCore.Storage;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Domain.Common;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Infrastructure.Data;
using SmartRoutine.Infrastructure.Services;

namespace SmartRoutine.Infrastructure.Common;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventService _domainEventService;
    private IDbContextTransaction? _currentTransaction;

    private IRepository<User>? _users;
    private IRepository<Routine>? _routines;
    private IRepository<RoutineLog>? _routineLogs;

    public UnitOfWork(ApplicationDbContext context, IDomainEventService domainEventService)
    {
        _context = context;
        _domainEventService = domainEventService;
    }

    public IRepository<User> Users => _users ??= new RepositoryAdapter<User>(_context);
    public IRepository<Routine> Routines => _routines ??= new RepositoryAdapter<Routine>(_context);
    public IRepository<RoutineLog> RoutineLogs => _routineLogs ??= new RepositoryAdapter<RoutineLog>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get all entities with domain events
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Collect all domain events
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear domain events from entities
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // Save changes
        var result = await _context.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful save
        if (domainEvents.Any())
        {
            await _domainEventService.PublishAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _currentTransaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _currentTransaction.RollbackAsync();
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context?.Dispose();
    }
}

// Adapter to bridge between different IRepository interfaces
internal class RepositoryAdapter<T> : IRepository<T> where T : class
{
    private readonly Repositories.Repository<T> _repository;

    public RepositoryAdapter(ApplicationDbContext context)
    {
        _repository = new Repositories.Repository<T>(context);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<T> UpdateAsync(T entity)
    {
        return await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(T entity)
    {
        await _repository.DeleteAsync(entity);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null;
    }

    public async Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate, string? includeProperties = null)
    {
        return await _repository.FindAsync(predicate, includeProperties);
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate)
    {
        return await _repository.ExistsAsync(predicate);
    }
} 