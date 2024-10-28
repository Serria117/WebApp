using Microsoft.AspNetCore.SignalR;
using WebApp.SignalrConfig;

namespace WebApp.Services.NotificationService;

public interface INotificationAppService
{
    void RegisterConnection(string? userId, string connectionId);
    void UnregisterConnection(string? userId);
    Task SendNotificationAsync(string? userId, string method, object message);
}

public class NotificationAppService(IHubContext<AppHub> hubContext) : INotificationAppService
{
    private readonly Dictionary<string?, string> _userConnections = new();

    public void RegisterConnection(string? userId, string connectionId)
    {
        _userConnections.Add(userId, connectionId);
    }

    public void UnregisterConnection(string? userId)
    {
        _userConnections.Remove(userId);
    }

    public async Task SendNotificationAsync(string? userId, string method, object message)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            await hubContext.Clients.Client(connectionId).SendAsync(method, message);
        }
    }
}