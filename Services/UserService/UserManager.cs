using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApp.Services.UserService;

public interface IUserManager
{
    string? CurrentUsername();
    string CurrentUserId();
}

public class UserManager(IHttpContextAccessor httpContextAccessor) : IUserManager
{
    private readonly HttpContext? _httpContext = httpContextAccessor.HttpContext;

    public string? CurrentUsername()
    {
        return _httpContext?.User.Identity?.Name;
    }

    public string CurrentUserId()
    {
        const string claimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        return _httpContext?.User.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
    }
    
    //TODO: add tenant, working organizations, etc...
}