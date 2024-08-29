using WebApp.Repositories;

namespace WebApp.Services;

public abstract class BaseService(IConfiguration configuration, HttpContext httpContext)
{
    public string? CurrentUser()
    {
        return httpContext.User.Identity!.Name;
    }
}