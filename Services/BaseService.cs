using Microsoft.AspNetCore.Mvc;
using WebApp.Repositories;
using WebApp.Services.UserService;

namespace WebApp.Services;

public class AppServiceBase(IUserManager userManager)
{
    protected IUserManager UserManager { get; set; } = userManager;
}