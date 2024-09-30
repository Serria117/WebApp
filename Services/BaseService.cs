using Microsoft.AspNetCore.Mvc;
using WebApp.Repositories;

namespace WebApp.Services;

public abstract class AppServiceBase()
{
    public string? CurrentUsername([FromServices] IHttpContextAccessor contextAccessor)
    {
        return contextAccessor.HttpContext?.User.Identity!.Name;
    }

    public string? CurrentUserId([FromServices] IHttpContextAccessor contextAccessor)
    {
        var user = contextAccessor.HttpContext?.User;
        var id = user?.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        return id;
    }
}