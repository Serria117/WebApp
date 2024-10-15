using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.OrganizationService;
using WebApp.Services.OrganizationService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/org")]
[Authorize]
public class OrganizationController(IOrganizationAppService orgService) : ControllerBase
{
    /// <summary>
    /// Create a new organization
    /// </summary>
    /// <remarks></remarks>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("create")]
    [HasAuthority(Permissions.OrgCreate)]
    public async Task<IActionResult> Create(OrganizationInputDto input)
    {
        var res = await orgService.Create(input);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    /// <summary>
    /// Create many organizations at once
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("create-many")]
    public async Task<IActionResult> CreateMany(List<OrganizationInputDto> input)
    {
        var res = await orgService.CreateMany(input);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    /// <summary>
    /// Get a list of organizations
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpGet("all")]
    [HasAuthority(Permissions.OrgView)]
    public async Task<IActionResult> GetAll([FromQuery] RequestParam req)
    {
        var page = PageRequest.GetPage(req);
        var res = await orgService.Find(page);
        return Ok(res);
    }

    /// <summary>
    /// Get a single organization
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")] [HasAuthority(Permissions.OrgView)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var res = await orgService.GetOneById(id);
        return res.Success ? Ok(res) : NotFound(res);
    }

    /// <summary>
    /// Update organization's info
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id:guid}")]
    [HasAuthority(Permissions.OrgUpdate)]
    public async Task<IActionResult> Update(Guid id, OrganizationInputDto dto)
    {
        var result = await orgService.Update(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Check for duplicated taxId
    /// </summary>
    /// <param name="taxId"></param>
    /// <returns></returns>
    [HttpGet("exist/{taxId}")]
    [HasAuthority(Permissions.OrgView)]
    public async Task<AppResponse> CheckTaxId(string taxId)
    {
        return await orgService.CheckTaxIdExist(taxId);
    }
}