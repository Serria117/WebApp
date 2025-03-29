using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;

namespace WebApp.Services.DocumentService;

public interface IDocumentAppService
{
    Task<AppResponse> GetFilesByOrgAndTypeAsync(Guid orgId, DocumentType documentType);
    Task<AppResponse> UploadDocFileAsync(string orgId, List<IFormFile> files);
    Task<AppResponse> GetFileByIdAsync(Guid orgId, int documentId);
}

public class DocumentAppService(IAppRepository<OrgDocument, int> docRepository,
                                IAppRepository<Organization, Guid> orgRepository,
                                ILogger<DocumentAppService> logger,
                                IHostEnvironment env) : IDocumentAppService
{
    public async Task<AppResponse> UploadDocFileAsync(string orgId, List<IFormFile> files)
    {
        if (files.Count == 0) return AppResponse.Error("No file uploaded");
        var org = await orgRepository.FindByIdAsync(Guid.Parse(orgId));
        if (org is null) return AppResponse.Error404("Organization not found");

        var uploadFiles = new List<OrgDocument>();
        var uploadDir = Path.Combine(env.ContentRootPath, "Uploads", org.TaxId);

        if (!Directory.Exists(uploadDir))
        {
            Directory.CreateDirectory(uploadDir);
        }
        
        var validationError = new List<string>();

        foreach (var file in files)
        {
            if (!(await ValidateDocument(org, file)))
            {
                validationError.Add(file.FileName);
                continue;
            }
            var uniqueFileName = $"{file.FileName}_{Ulid.NewUlid()}.xml";
            var filePath = Path.Combine(uploadDir, uniqueFileName);
            
            // save the file to the stream
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                //await stream.DisposeAsync();
            }
            // read document and extract some extra metadata:
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var doc = await XDocument.LoadAsync(fileStream, LoadOptions.None, CancellationToken.None);
            var type = GetDocumentTypeFromXml(doc);
            var soLan = doc.GetXmlNodeValue("soLan");
            var docDate = doc.GetXmlNodeValue("ngayLapTKhai");
            
            // create the document object
            var uploadFile = new OrgDocument
            {
                Organization = org,
                DocumentType = type,
                DocumentName = doc.GetXmlNodeValue("tenTKhai"),
                DocumentDate = docDate == null
                    ? null
                    : DateTime.ParseExact(docDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                AdjustmentType = doc.GetXmlNodeValue("loaiTKhai"),
                NumberOfAdjustment = soLan is null ? 0 : int.Parse(soLan),
                Period = doc.GetXmlNodeValue("kyKKhai"),
                PeriodType = doc.GetXmlNodeValue("kieuKy"),
                FileName = file.FileName,
                FilePath = filePath,
                UploadTime = DateTime.Now,
                FileSize = file.Length
            };

            uploadFiles.Add(uploadFile);
        }

        var total = uploadFiles.Count;
        await docRepository.CreateManyAsync(uploadFiles);
        return AppResponse.SuccessResponse(new
        {
            message = $"{total} file(s) uploaded successfully.",
            total,
            error = validationError.Count,
            validationError
        });
    }

    public async Task<AppResponse> GetFilesByOrgAndTypeAsync(Guid orgId, DocumentType documentType)
    {
        var org = await orgRepository.FindByIdAsync(orgId);
        if (org is null) return AppResponse.Error404("Organization not found");

        var files = await docRepository.FindAndSort(x => x.Organization.Id == orgId && x.DocumentType == documentType,
                                                    [],
                                                    ["CreateAt DESC"])
                                       .ToListAsync();

        return AppResponse.SuccessResponse(files.Select(f => new
        {
            f.Id,
            f.FileName,
            f.FilePath,
            f.UploadTime,
            DocumentType = f.DocumentType.ToString(),
            Name = f.DocumentName,
            f.Period,
            f.NumberOfAdjustment,
            f.PeriodType,
            f.DocumentDate,
        }));
    }

    public async Task<AppResponse> GetFileByIdAsync(Guid orgId, int documentId)
    {
        var f = await docRepository.Find(filter: x => x.Organization.Id == orgId && x.Id == documentId,
                                         include: "Organization")
                                   .FirstOrDefaultAsync();
        if (f is not null)
        {
            return AppResponse.SuccessResponse(f.FilePath);
        }

        return AppResponse.Error404("Document not found");
    }

    private DocumentType GetDocumentTypeFromXml(XDocument doc)
    {
        try
        {
            // find the node 'maTKhai' to extract document type:
            var maTKhai = doc.GetXmlNodeValue("maTKhai");
            if (string.IsNullOrEmpty(maTKhai))
            {
                throw new Exception("maTKhai not found");
            }

            return maTKhai switch
            {
                "892" => DocumentType.TK_03TNDN,
                "842" => DocumentType.TK_01GTGT,
                "864" => DocumentType.TK_05KK_TNCN,
                "953" => DocumentType.TK_05QTN_TNCN,
                "683" => DocumentType.TK_BCTC_133,
                _ => DocumentType.General_doc
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while getting document type from xml");
            return DocumentType.Undefined;
        }
    }

    private async Task<bool> ValidateDocument(Organization org, IFormFile file)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
            var taxId = document.GetXmlNodeValue("mst");
            return taxId is null || taxId == org.TaxId;
        }
        catch (Exception e)
        {
            logger.LogError("Error while validating document. Caused by: {}", e.Message);
            return false;
        }
    }
}