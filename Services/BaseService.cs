using Microsoft.AspNetCore.Mvc;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Services.Mappers;
using WebApp.Services.UserService;

namespace WebApp.Services;

public class AppServiceBase(IUserManager userManager)
{
    protected IUserManager UserManager { get; } = userManager;
    /// <summary>
    /// Get the current user's id
    /// </summary>
    protected string UserId { get; } = userManager.CurrentUserId();
}