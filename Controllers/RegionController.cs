using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Payloads;
using WebApp.Services.RegionService;
using WebApp.Services.RegionService.Dto;

namespace WebApp.Controllers;

[ApiController] [Route("/api/region")] [Authorize]
public class RegionController(IRegionAppService regionService) : ControllerBase
{
    [HttpGet("provinces")]
    public async Task<IActionResult> GetProvince([FromQuery] RequestParam req)
    {
        var page = PageRequest.GetPage(req.Valid());
        var result = await regionService.GetAllProvincesAsync(page);
        return Ok(result);
    }
    
    [HttpPost("province")]
    public async Task<IActionResult> CreateProvince(ProvinceCreateDto input)
    {
        return Ok(await regionService.CreateProvinceAsync(input));
    }
    
    [HttpPost("provinces")]
    public async Task<IActionResult> CreateManyProvinces(List<ProvinceCreateDto> input)
    {
        var result = await regionService.CreateManyProvincesAsync(input);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("district")]
    public async Task<IActionResult> CreateDistrict(DistrictCreateDto input)
    {
        return Ok(await regionService.CreateDistrictAsync(input));
    }

    [HttpPost("districts/{pId:int}")]
    public async Task<IActionResult> CreateManyDistricts(int pId, List<DistrictCreateDto> input)
    {
        var result = await regionService.CreateManyDistrictsAsync(pId, input);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("taxOffice")]
    public async Task<IActionResult> CreateTaxOffice(TaxOfficeCreateDto input)
    {
        return Ok(await regionService.CreateTaxOfficeAsync(input));
    }

    [HttpPost("taxOffices/{pId:int}")]
    public async Task<IActionResult> CreateManyTaxOffices(int pId, List<TaxOfficeCreateDto> input)
    {
        var result = await regionService.CreateManyTaxOfficeAsync(pId, input);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}