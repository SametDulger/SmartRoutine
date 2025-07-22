using MediatR;

namespace SmartRoutine.Domain.Common;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
} 