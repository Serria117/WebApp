using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebApp.Core.DomainEntities;
using WebApp.Services.NotificationService;

namespace WebApp.SignalrConfig;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AppHub(INotificationAppService notificationService) : Hub
{
    /// <summary>
    /// Called when a new connection is established.
    /// 
    /// We register the user connection id to the notification service.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
        {
            notificationService.RegisterConnection(userId, Context.ConnectionId);
        }
        return base.OnConnectedAsync();
    }
    
    /// <summary>
    /// Called when a connection is terminated.
    /// 
    /// We unregister the user connection id from the notification service.
    /// </summary>
    /// <param name="exception">The exception associated with the disconnection event, if any.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId is not null) 
            notificationService.UnregisterConnection(userId);
        Console.WriteLine($"Hub Disconnected: {exception?.Message}");
        return base.OnDisconnectedAsync(exception);
    }
};