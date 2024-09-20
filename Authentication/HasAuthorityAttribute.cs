using Microsoft.AspNetCore.Authorization;

namespace WebApp.Authentication;

public sealed class HasAuthorityAttribute : AuthorizeAttribute
{
    public HasAuthorityAttribute(string permission) : base(policy: permission)
    {
    }
}
