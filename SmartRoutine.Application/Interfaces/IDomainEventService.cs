using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
} 