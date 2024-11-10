using Microsoft.AspNetCore.SignalR;
using WebApp.SignalrConfig;

namespace WebApp.Services.NotificationService;

public interface INotificationAppService
{
    void RegisterConnection(string userId, string connectionId);
    void UnregisterConnection(string userId);
    Task SendAsync(string userId, string method, object message);
}

public class NotificationAppService(IHubContext<AppHub> hubContext) : INotificationAppService
{
    private readonly Dictionary<string, string> _userConnections = new();

    /// <summary>
    /// Registers a connection for a user by storing the user's connection ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="connectionId">The connection ID associated with the user.</param>
    public void RegisterConnection(string userId, string connectionId)
    {
        _userConnections[userId] = connectionId;
    }

    /// <summary>
    /// Unregisters a connection for a user by removing the user's connection ID from the internal storage.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to unregister the connection for.</param>
    public void UnregisterConnection(string userId)
    {
        _userConnections.Remove(userId);
    }

/// <summary>
/// Sends a message to a specific user using their connection ID.
/// </summary>
/// <param name="userId">The unique identifier of the user to send the message to.</param>
/// <param name="method">The method name to invoke on the client.</param>
/// <param name="message">The message object to be sent to the user.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendAsync(string userId, string method, object message)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            await hubContext.Clients.Client(connectionId).SendAsync(method, message);
        }
    }
}