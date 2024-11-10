using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.BalanceSheetService;
using WebApp.Services.BalanceSheetService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("api/account-template")] 
[Authorize]
public class AccountTemplateController(IBalanceSheetAppService balanceSheetService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> AccountTemplate()
    {
        var result = await balanceSheetService.GetAccountTemplate();
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAccountTemplate(AccountCreateDto input)
    {
        var result = await balanceSheetService.CreateAccountTemplate(input);
        return Ok(result);
    }
}