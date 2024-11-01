using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebApp.Core.DomainEntities;
using WebApp.Services.NotificationService;

namespace WebApp.SignalrConfig;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AppHub(INotificationAppService notificationService) : Hub
{
    public override Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
        {
            notificationService.RegisterConnection(userId, Context.ConnectionId);
        }
        return base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId is not null) 
            notificationService.UnregisterConnection(userId);
        Console.WriteLine($"Hub Disconnected: {exception?.Message}");
        return base.OnDisconnectedAsync(exception);
    }
};