using MediatR;
using Microsoft.Extensions.Logging;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;
using SmartRoutine.Application.Interfaces;

namespace SmartRoutine.Application.EventHandlers;

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;
    private readonly INotificationService _notificationService;

    public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("New user registered: {UserId} - {Email}",
            notification.UserId, notification.Email);

        // Kullanıcıya hoş geldin e-postası gönder
        await _notificationService.SendWelcomeEmailAsync(
            email: notification.Email,
            userName: notification.Email.Split('@')[0]
        );
    }
} 