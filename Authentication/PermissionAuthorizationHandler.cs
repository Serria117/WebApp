using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApp.Services.UserService;

namespace WebApp.Authentication;

public class PermissionAuthorizationHandler(
    IServiceScopeFactory serviceScopeFactory, 
    ILogger<PermissionAuthorizationHandler> log) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var stringUserId = context.User.Claims
            .FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(stringUserId, out var id))
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            IPermissionAppService permissionService = scope.ServiceProvider.GetRequiredService<IPermissionAppService>();

            //Try to get permission from noSQL storage for faster performance
            var permissions = await permissionService.GetPermissionsFromMongo(id);

            //In case user does not exist in noSQL storage, retrieve from db
            if(permissions is null || permissions.Count == 0)
            {
                log.LogWarning("User not found in mongoDb storage. Retrieving user from database...");
                permissions = await permissionService.GetPermissions(id);
            }

            //Check if user's permissions contains the required permission to access endpoint
            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }

    }
}
