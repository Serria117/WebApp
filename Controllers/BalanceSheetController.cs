using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Payloads;
using WebApp.Services.BalanceSheetService;
using WebApp.Services.BalanceSheetService.Dto;

namespace WebApp.Controllers;

[ApiController, Authorize, Route("api/balance-sheet")]
public class BalanceSheetController(IBalanceSheetAppService service) : ControllerBase
{
    /// <summary>
    /// Create a balance sheet from user's imported data
    /// </summary>
    /// <response code="200">Returns the success message</response>
    /// <response code="400">If the input params is invalid or null</response>          
    /// <param name="orgId"></param>
    /// <param name="bsParam"></param>
    /// <returns>The creation result</returns>
    [HttpPost("{orgId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBalanceSheet(Guid orgId, BalanceSheetParams bsParam)
    {
        var result = await service.CreateImportedBalanceSheet(orgId, bsParam);
        return Ok(result);
    }

    /// <summary>
    /// Get the list of imported balance sheet of an organization
    /// </summary>
    /// <param name="orgId"></param>
    /// <returns>The list of all balance sheet of an organization</returns>
    [HttpGet("{orgId:guid}")]
    public async Task<IActionResult> GetBalanceSheets(Guid orgId)
    {
        var result = await service.GetImportedBalanceSheetsByOrg(orgId);
        return Ok(result);
    }

    /// <summary>
    /// Get a single imported balance sheet and its details
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The balance sheet and full details</returns>
    [HttpPost("detail/{id:int}")]
    public async Task<IActionResult> GetBalanceSheetDetail(int id)
    {
        var result = await service.GetImportedBalanceSheets(id);
        return Ok(result);
    }

    /// <summary>
    /// Upload balance sheet file (excel .xlsx)
    /// </summary>
    /// <param name="orgId">Organization Id</param>
    /// <param name="year">Year of the report</param>
    /// <param name="file">Excel file template</param>
    /// <returns></returns>
    [HttpPost("upload/{orgId:guid}")]
    public async Task<IActionResult> Upload(Guid orgId, [FromForm] int year, IFormFile file)
    {
        var res = await service.ProcessBalanceSheet(orgId, year, file);
        return Ok(res);
    }
}