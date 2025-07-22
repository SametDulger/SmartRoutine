using Microsoft.AspNetCore.SignalR;

namespace SmartRoutine.API.Hubs;

public class NotificationHub : Hub
{
    // Kullanıcıya özel mesaj göndermek için metot
    public async Task SendRoutineUpdate(string userId, string message)
    {
        await Clients.User(userId).SendAsync("RoutineUpdated", message);
    }
} 