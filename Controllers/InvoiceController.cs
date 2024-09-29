﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.InvoiceService;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService;
using WebApp.Services.RestService.Dto;

namespace WebApp.Controllers;

[ApiController] [Route("/api/invoice")] [Authorize]
public class InvoiceController(IRestAppService restService,
                               IInvoiceAppService invService) : ControllerBase
{
    [HttpGet("capcha-login")]
    public async Task<IActionResult> GetCapcha()
    {
        var result = await restService.GetCapcha();
        return Ok(result);
    }

    [HttpPost("capcha-login")]
    public async Task<IActionResult> LoginInvoiceService(InvoiceLoginModel loginModel)
    {
        var result = await restService.Authenticate(loginModel);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("find/{taxId}")]
    public async Task<IActionResult> FindInvoice(string taxId, [FromQuery] InvoiceParams parameters)
    {
        var result = await invService.GetInvoices(taxId, parameters.Valid());
        return Ok(result);
    }

    [HttpGet("extract")]
    public async Task<IActionResult> ExtractInvoice(string token, string from, string to)
    {
        var result = await restService.GetInvoiceListAsync(token, from, to);
        if (result is not { Success: true, Data: not null }) return BadRequest(result);

        List<InvoiceDisplayDto> details = [];
        foreach (var invoice in (List<InvoiceDisplayDto>)result.Data)
        {
            var invDetail = await restService.GetInvoiceDetail(token, invoice);
            if (invDetail is { Success: true, Data: not null })
                details.Add((InvoiceDisplayDto)invDetail.Data);
        }

        return Ok(AppResponse.SuccessResponse(details));
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncInvoice(SyncInvoiceRequest request)
    {
        var res = await invService.SyncInvoices(request.Token, request.From, request.To);
        return Ok(res);
    }

    [HttpGet("download/{taxId}")]
    public async Task<IActionResult> DownloadInvoice(string taxId, string from, string to)
    {
        var stream = await invService.ExportExcel(taxId, from, to);
        var fileName = $"{taxId}_{from}_{to}.xlsx";
        Response.Headers["X-Filename"] = fileName;
        return File(stream, ContentType.ApplicationOfficeXlsx, fileName);
    }
}