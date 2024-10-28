using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.InvoiceService;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService;
using WebApp.Services.RestService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/invoice")] [Authorize]
public class InvoiceController(IRestAppService restService,
                               IInvoiceAppService invService) : ControllerBase
{
    /// <summary>
    /// Get capcha data from hoadondientu.gdt.gov.vn for login
    /// </summary>
    /// <returns></returns>
    [HttpGet("capcha-login")]
    public async Task<IActionResult> GetCaptcha()
    {
        var result = await restService.GetCaptcha();
        return Ok(result);
    }

    /// <summary>
    /// Authenticate with hoadondientu.gdt.gov.vn
    /// </summary>
    /// <param name="loginModel"></param>
    /// <returns></returns>
    [HttpPost("capcha-login")]
    public async Task<IActionResult> LoginInvoiceService(InvoiceLoginModel loginModel)
    {
        var result = await restService.Authenticate(loginModel);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Find and get invoices list in database
    /// </summary>
    /// <param name="taxId"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    [HttpGet("find/{taxId}")]
    public async Task<IActionResult> FindInvoice(string taxId, [FromQuery] InvoiceRequestParam parameters)
    {
        var result = await invService.FindPurchaseInvoices(taxId, parameters.Valid());
        return Ok(result);
    }

    /*/// <summary>
    /// Get invoices from hoadondientu.gdt.gov.vn
    /// </summary>
    /// <param name="token"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
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
    }*/

    /// <summary>
    /// Sync invoices with detail from hoadondientu.gdt.gov.vn
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("sync")]
    public async Task<IActionResult> SyncInvoice(SyncInvoiceRequest request)
    {
        var res = await invService.ExtractPurchaseInvoices(request.Token, request.From, request.To);
        return Ok(res);
    }

    /// <summary>
    /// Download invoices list as Excel file
    /// </summary>
    /// <param name="taxId"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns>The Excel file contains invoices in search range</returns>
    [HttpGet("download/{taxId}")]
    public async Task<IActionResult> DownloadInvoice(string taxId, string from, string to)
    {
        var fileByte = await invService.ExportExcel(taxId, from, to);
        var fileName = $"{taxId}_{from}_{to}_{Ulid.NewUlid()}.xlsx";
        Response.Headers["X-Filename"] = fileName;
        return File(fileByte, ContentType.ApplicationOfficeSpreadSheet, fileName);
    }

    /// <summary>
    /// Recheck if invoices status has changed
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The number of invoice that has been updated</returns>
    [HttpPost("recheck")]
    public async Task<IActionResult> RecheckInvoice(SyncInvoiceRequest request)
    {
        var result = await invService.RecheckPurchaseInvoice(request.Token, request.From, request.To);
        return result.Code == "207"
            ? StatusCode(StatusCodes.Status207MultiStatus, result)
            : Ok(result);
    }

    /// <summary>
    /// Get a single invoice value
    /// </summary>
    /// <param name="taxId">Organization's taxId</param>
    /// <param name="id">Invoice id</param>
    /// <returns></returns>
    [HttpGet("get/{taxId}")]
    public async Task<IActionResult> GetInvoice(string taxId, string id)
    {
        var result = await invService.FindOne(taxId, id);
        return result.Code == "200" ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Retrieve sold invoices from hoadondientu service
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <returns>Number of successfully added invoice (duplicate value will be ignored)</returns>
    [HttpPost("sold-invoice/sync")]
    public async Task<IActionResult> RetrieveSoldInvoice(SyncInvoiceRequest request)
    {
        var result = await invService.ExtractSoldInvoice(request);
        return Ok(result);
    }

    /// <summary>
    /// Find sold invoice in the database
    /// </summary>
    /// <param name="taxId">Organization taxid</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>List of sold invoice</returns>
    [HttpGet("sold/get-all/{taxId}")]
    public async Task<IActionResult> FindSoldInvoices(string taxId, 
                                                      [FromQuery] InvoiceRequestParam parameters)
    {
        var result = await invService.FindSoldInvoices(taxId, parameters);
        return Ok(result);
    }
}