using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Services.RiskCompanyService;

namespace WebApp.Controllers;

[ApiController, Route("api/risk")] [Authorize]
public class RiskCompanyController(IRiskCompanyAppService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRiskCompanies([FromQuery] RequestParam req)
    {
        var page = PageRequest.GetPage(req);
        var result = await service.GetAsync(page);
        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateRiskCompany(RiskCompany company)
    {
        var result = await service.CreateAsync(company);
        return Ok(result);
    }

    [HttpPost("create-many")]
    public async Task<IActionResult> CreateManyRiskCompanies(List<RiskCompany> company)
    {
        var result = await service.CreateManyAsync(company);
        return Ok(result);
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var result = await service.SoftDeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("delete-many")]
    public async Task<IActionResult> DeleteMany(List<int> ids)
    {
        var result = await service.SoftDeleteManyAsync(ids);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}