using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Enums;
using WebApp.Services.DocumentService;

namespace WebApp.Controllers;

[ApiController, Route("api/document")]
[Authorize]
public class OrgDocumentController(IDocumentAppService docService, IHostEnvironment env) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(string orgId, List<IFormFile> file)
    {
        var response = await docService.UploadDocFileAsync(orgId, file);
        return Ok(response);
    }

    [HttpGet("get-document")]
    public async Task<IActionResult> GetFilesByOrgAndType([FromQuery] Guid orgId,
                                                          [FromQuery] DocumentType documentType)
    {
        var response = await docService.GetFilesByOrgAndTypeAsync(orgId, documentType);
        return Ok(response);
    }

    [HttpGet("download/{orgId:guid}")]
    public async Task<IActionResult> DownloadDocument(Guid orgId, [FromQuery] int documentId)
    {
        var fileResponse = await docService.GetFileByIdAsync(orgId, documentId);
        if (!fileResponse.Success || fileResponse.Data is null)
        {
            return NotFound("File not found or you don't have permission to access the file");
        }
        
        string filePath = Path.Combine(env.ContentRootPath, (string) fileResponse.Data);
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("The file may have been moved or deleted");
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        const string contentType = "application/xml";
        var fileName = Path.GetFileName(filePath);
        return File(fileStream, contentType, fileName);
    }
}