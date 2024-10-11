using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Services.BalanceSheetService;
using WebApp.Services.BalanceSheetService.Dto;

namespace WebApp.Controllers;

[ApiController, Authorize, Route("api/balance-sheet")]
public class BalanceSheetController(IBalanceSheetAppService service) : ControllerBase
{
    [HttpPost("{orgId:guid}")]
    public async Task<IActionResult> CreateBalanceSheet(Guid orgId, List<ImportedBsDetailCreateDto> details)
    {
        var result = await service.CreateImportedBalanceSheet(orgId, details);
        return Ok(result);
    }

    [HttpGet("{orgId:guid}")]
    public async Task<IActionResult> GetBalanceSheets(Guid orgId)
    {
        var result = await service.GetImportedBalanceSheetsByOrg(orgId);
        return Ok(result);
    }
    
    [HttpPost("detail/{id:int}")]
    public async Task<IActionResult> GetBalanceSheetDetail(int id)
    {
        var result = await service.GetImportedBalanceSheets(id);
        return Ok(result);
    }
}