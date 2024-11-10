using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Services.RiskCompanyService;

namespace WebApp.Controllers;

[ApiController, Route("api/risk")] [Authorize]
public class RiskCompanyController(IRiskCompanyAppService service) : ControllerBase
{
    /// <summary>
    /// Get all risk companies
    /// </summary>
    /// <param name="req">Query parameters</param>
    /// <returns>List of risk companies</returns>
    [HttpGet]
    public async Task<IActionResult> GetRiskCompanies([FromQuery] RequestParam req)
    {
        var page = PageRequest.GetPage(req);
        var result = await service.GetAsync(page);
        return Ok(result);
    }

    /// <summary>
    /// Create a new risk company
    /// </summary>
    /// <param name="company">The risk company entity to create</param>
    /// <returns>An IActionResult containing the created risk company</returns>
    [HttpPost("create")]
    public async Task<IActionResult> CreateRiskCompany(RiskCompany company)
    {
        var result = await service.CreateAsync(company);
        return Ok(result);
    }

    /// <summary>
    /// Create multiple risk companies at once
    /// </summary>
    /// <param name="company">A list of risk companies to create</param>
    /// <returns>An IActionResult containing the created risk companies</returns>
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