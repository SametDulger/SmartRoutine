using MediatR;
using Microsoft.Extensions.Logging;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;
using SmartRoutine.Application.Interfaces;

namespace SmartRoutine.Application.EventHandlers;

public class RoutineCompletedEventHandler : INotificationHandler<RoutineCompletedEvent>
{
    private readonly ILogger<RoutineCompletedEventHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IStatsService _statsService;

    public RoutineCompletedEventHandler(ILogger<RoutineCompletedEventHandler> logger, INotificationService notificationService, IStatsService statsService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _statsService = statsService;
    }

    public async Task Handle(RoutineCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Routine completed: {RoutineId} by user {UserId} - {Title} at {CompletedAt}",
            notification.RoutineId, notification.UserId, notification.Title, notification.CompletedAt);

        // Kullanıcıya rutin tamamlandığında tebrik e-postası gönder
        await _notificationService.SendRoutineCompletedEmailAsync(
            email: "", // Kullanıcı emaili burada alınmalı
            routineName: notification.Title,
            currentStreak: 0 // Örnek: streak bilgisi StatsService'den alınabilir
        );

        // İstatistikleri güncelle
        await _statsService.GetSummaryAsync(notification.UserId);
    }
} 