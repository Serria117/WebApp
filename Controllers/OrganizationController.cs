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
    [HttpPost("create")]
    [HasAuthority(Permissions.OrgCreate)]
    public async Task<IActionResult> Create(OrganizationInputDto input)
    {
        var res = await orgService.Create(input);
        return res.Success ? Ok(res) : BadRequest(res);
    }
    
    [HttpPost("create-many")]
    public async Task<IActionResult> CreateMany(List<OrganizationInputDto> input)
    {
        var res = await orgService.CreateMany(input);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    [HttpGet("all")]
    [HasAuthority(Permissions.OrgView)]
    public async Task<IActionResult> GetAll([FromQuery] RequestParam req)
    {
        var page = PageRequest.GetPage(req);
        var res = await orgService.Find(page);
        return Ok(res);
    }

    [HttpGet("{id:guid}")]
    [HasAuthority(Permissions.OrgView)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var res = await orgService.GetOneById(id);
        return res.Success ? Ok(res) : NotFound(res);
    }

    [HttpPut("{id:guid}")]
    [HasAuthority(Permissions.OrgUpdate)]
    public async Task<IActionResult> Update(Guid id, OrganizationInputDto dto)
    {
        var result = await orgService.Update(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("exist/{taxId}")]
    [HasAuthority(Permissions.OrgView)]
    public async Task<AppResponse> CheckTaxId(string taxId)
    {
        return await orgService.CheckTaxIdExist(taxId);
    }
}