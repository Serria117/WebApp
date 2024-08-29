using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApp.Services.UserService;

namespace WebApp.Authentication
{
    public class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory) : AuthorizationHandler<PermissionRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var stringUserId = context.User.Claims
                .FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(stringUserId, out var id))
            {
                using IServiceScope scope = serviceScopeFactory.CreateScope();
                IPermissionAppService permissionService = scope.ServiceProvider.GetRequiredService<IPermissionAppService>();

                var permissions = await permissionService.GetPermission(id);

                if (permissions.Contains(requirement.Permission))
                {
                    context.Succeed(requirement);
                }
            }

        }
    }
}
