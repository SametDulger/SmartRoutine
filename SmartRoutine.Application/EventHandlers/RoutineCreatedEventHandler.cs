using MediatR;
using Microsoft.Extensions.Logging;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;
using SmartRoutine.Application.Interfaces;

namespace SmartRoutine.Application.EventHandlers;

public class RoutineCreatedEventHandler : INotificationHandler<RoutineCreatedEvent>
{
    private readonly ILogger<RoutineCreatedEventHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IStatsService _statsService;

    public RoutineCreatedEventHandler(ILogger<RoutineCreatedEventHandler> logger, INotificationService notificationService, IStatsService statsService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _statsService = statsService;
    }

    public async Task Handle(RoutineCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Routine created: {RoutineId} by user {UserId} - {Title}",
            notification.RoutineId, notification.UserId, notification.Title);

        // Kullanıcıya rutin oluşturulduğunda hatırlatıcı e-posta gönder
        await _notificationService.SendRoutineReminderAsync(
            email: "", // Kullanıcı emaili burada alınmalı (örnek: UserService'den veya context'ten)
            routineName: notification.Title,
            scheduledTime: new TimeOnly(8, 0)); // Örnek saat

        // İstatistikleri güncelle (örnek: yeni rutin sayısı)
        await _statsService.GetSummaryAsync(notification.UserId);
    }
}

public class RoutineUpdatedEventHandler : INotificationHandler<RoutineUpdatedEvent>
{
    private readonly ILogger<RoutineUpdatedEventHandler> _logger;

    public RoutineUpdatedEventHandler(ILogger<RoutineUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(RoutineUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Routine updated: {RoutineId} by user {UserId} - {Title} at {TimeOfDay}",
            notification.RoutineId, notification.UserId, notification.Title, notification.TimeOfDay);

        // Here you could add additional logic like:
        // - Update cached data
        // - Send notification to user
        // - Update statistics
        // - Sync with external services

        await Task.CompletedTask;
    }
}

public class RoutineDeletedEventHandler : INotificationHandler<RoutineDeletedEvent>
{
    private readonly ILogger<RoutineDeletedEventHandler> _logger;

    public RoutineDeletedEventHandler(ILogger<RoutineDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(RoutineDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Routine deleted: {RoutineId} by user {UserId} - {Title}",
            notification.RoutineId, notification.UserId, notification.Title);

        // Here you could add additional logic like:
        // - Clean up cached data
        // - Remove reminders
        // - Update statistics
        // - Archive data for analytics

        await Task.CompletedTask;
    }
} 