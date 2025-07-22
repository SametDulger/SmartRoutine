using MediatR;
using Microsoft.Extensions.Logging;
using SmartRoutine.Domain.Common;
using SmartRoutine.Application.Interfaces;

namespace SmartRoutine.Infrastructure.Services;

public class DomainEventService : IDomainEventService
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventService> _logger;

    public DomainEventService(IMediator mediator, ILogger<DomainEventService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing domain event: {EventType} with ID: {EventId}", 
            domainEvent.GetType().Name, domainEvent.Id);

        await _mediator.Publish(domainEvent, cancellationToken);
    }

    public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await PublishAsync(domainEvent, cancellationToken);
        }
    }
} 