using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Spire.Xls;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Services.Mappers;
using WebApp.Services.UserService;

namespace WebApp.Services.BalanceSheetService;

public interface IBalanceSheetAppService
{
    Task<AppResponse> ProcessBalanceSheet(Guid orgId, int year, IFormFile file);
    Task<AppResponse> CreateImportedBalanceSheet(Guid orgId, BalanceSheetParams input);
    Task<AppResponse> GetImportedBalanceSheetsByOrg(Guid orgId);
    Task<AppResponse> GetImportedBalanceSheets(int id);
    Task DeleteImportedBalanceSheet(int id);
    Task<AppResponse> HardDeleteImportedBalanceSheet(int id);
}

public class BalanceSheetAppService(IAppRepository<Account, int> accountRepo,
                                    IAppRepository<BalanceSheet, int> balanceSheetRepo,
                                    IAppRepository<BalanceSheetDetail, int> balanceSheetDetailRepo,
                                    IAppRepository<ImportedBalanceSheet, int> importedBsRepo,
                                    IAppRepository<ImportedBalanceSheetDetail, int> importedBsDetailRepo,
                                    IAppRepository<Organization, Guid> orgRepo,
                                    ILogger<BalanceSheetAppService> logger,
                                    IUserManager userManager) : IBalanceSheetAppService
{
    public async Task<AppResponse> CreateImportedBalanceSheet(Guid orgId, BalanceSheetParams input)
    {
        if (!await orgRepo.ExistAsync(o => o.Id == orgId)) return AppResponse.ErrorResponse("org id cannot be found");
        var balanceSheet = new ImportedBalanceSheet
        {
            Organization = orgRepo.Attach(orgId),
            Details = input.Details.MapCollection(x => x.ToEntity()).ToHashSet(),
        };

        CalculateBalanceSheetTotal(balanceSheet);

        var savedBs = await importedBsRepo.CreateAsync(balanceSheet);

        return AppResponse.SuccessResponse(new
        {
            savedBs.Id,
            Valid = savedBs.IsValid,
            OpenCr = savedBs.SumOpenCredit,
            OpenDr = savedBs.SumOpenDebit,
            AriseCr = savedBs.SumAriseCredit,
            AriseDr = savedBs.SumAriseDebit,
            CloseCr = savedBs.SumCloseCredit,
            CloseDr = savedBs.SumCloseDebit,
        });
    }

    public async Task<AppResponse> GetImportedBalanceSheetsByOrg(Guid orgId)
    {
        var result = await importedBsRepo
                           .Find(x => x.Organization != null && x.Organization.Id == orgId && !x.Deleted,
                                 sortBy: nameof(ImportedBalanceSheet.CreateAt),
                                 order: SortOrder.DESC)
                           .ToListAsync();
        return result.IsNullOrEmpty()
            ? AppResponse.ErrorResponse(ResponseMessage.NotFound)
            : AppResponse.SuccessResponse(result.MapCollection(x => x.ToDisplayDto()));
    }

    public async Task<AppResponse> GetImportedBalanceSheets(int id)
    {
        var result = await importedBsRepo
                           .Find(x => x.Id == id && !x.Deleted, include: [nameof(ImportedBalanceSheet.Details)])
                           .FirstOrDefaultAsync();
        
        if(result is null) return AppResponse.ErrorResponse(ResponseMessage.NotFound);
        
        return AppResponse.SuccessResponse(result.ToDisplayDto());
    }

    public async Task DeleteImportedBalanceSheet(int id)
    {
        await importedBsRepo.SoftDeleteAsync(id);
        await importedBsDetailRepo.Find(x => x.ImportedBalanceSheet.Id == id)
                                  .ExecuteUpdateAsync(x => x.SetProperty(b => b.Deleted, true));
    }

    public async Task<AppResponse> ProcessBalanceSheet(Guid orgId, int year, IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var importedBalanceSheet = new ImportedBalanceSheet
        {
            Details = ReadBalanceSheetFromFile(stream).ToHashSet(),
        };

        return AppResponse.SuccessResponse(new
        {
            ImportedBalanceSheet = importedBalanceSheet,
            BalanceSheet = new BalanceSheet
            {
                ImportedBalanceSheet = importedBalanceSheet,
            }
        });
    }

    public async Task<AppResponse> HardDeleteImportedBalanceSheet(int id)
    {
        var result = await importedBsRepo.HardDeleteAsync(id);
        return result ? AppResponse.Ok() : AppResponse.ErrorResponse(ResponseMessage.NotFound);
    }

    private static void CalculateBalanceSheetTotal(ImportedBalanceSheet bs)
    {
        foreach (var b in bs.Details.Where(b => b.Account?.ToString().Length == 3))
        {
            bs.SumOpenCredit += b.OpenCredit;
            bs.SumOpenDebit += b.OpenDebit;
            bs.SumCloseCredit += b.CloseCredit;
            bs.SumCloseDebit += b.CloseDebit;
            bs.SumAriseCredit += b.AriseCredit;
            bs.SumAriseDebit += b.AriseDebit;
        }

        bs.IsValid = bs.SumOpenCredit == bs.SumOpenDebit
                     && bs.SumAriseCredit == bs.SumAriseDebit
                     && bs.SumCloseCredit == bs.SumCloseDebit;
    }

    private List<ImportedBalanceSheetDetail> ReadBalanceSheetFromFile(Stream fileStream)
    {
        List<ImportedBalanceSheetDetail> balanceSheetDetails = [];

        using var workbook = new Workbook();
        workbook.LoadFromStream(fileStream);
        var worksheet = workbook.Worksheets[0];
        //Try to find the cell that contain the "111" account number to start with:
        var cell111 = worksheet.FindAll("111", FindType.Number, ExcelFindOptions.MatchEntireCellContent)
                               .FirstOrDefault();
        var startRow = cell111?.Row ?? 3;

        for (var i = startRow; i <= worksheet.Rows.Length; i++)
        {
            var detail = new ImportedBalanceSheetDetail
            {
                Account = worksheet.Range[i, 1].Value,
                OpenCredit = ParseDecimal(worksheet.Range[i, 2].Value),
                OpenDebit = ParseDecimal(worksheet.Range[i, 3].Value),
                AriseCredit = ParseDecimal(worksheet.Range[i, 4].Value),
                AriseDebit = ParseDecimal(worksheet.Range[i, 5].Value),
                CloseCredit = ParseDecimal(worksheet.Range[i, 6].Value),
                CloseDebit = ParseDecimal(worksheet.Range[i, 7].Value),
            };
            logger.LogInformation("{detail}", detail.ToString());
            balanceSheetDetails.Add(detail);
        }

        return balanceSheetDetails;
    }

    private async Task<BalanceSheetDetail> MapBalanceSheetDetail(ImportedBalanceSheetDetail imported)
    {
        var accounts = await accountRepo.FindAndSort(x => !x.Deleted, [], [nameof(Account.AccountNumber)])
                                        .Select(x => x.AccountNumber)
                                        .ToListAsync();

        if (imported.Account is null || !accounts.Contains(imported.Account))
        {
            imported.IsValid = false;
        }

        var existedDetail = await balanceSheetDetailRepo.Find(x => x.Account == imported.Account)
                                                        .FirstOrDefaultAsync();
        if (existedDetail is not null)
        {
            existedDetail.OpenCredit = imported.OpenDebit;
            existedDetail.OpenDebit = imported.OpenDebit;
            existedDetail.AriseCredit = imported.AriseDebit;
            existedDetail.AriseDebit = imported.AriseDebit;
            existedDetail.CloseCredit = imported.CloseDebit;
            existedDetail.CloseDebit = imported.CloseDebit;
            return await balanceSheetDetailRepo.UpdateAsync(existedDetail);
        }

        var newDetail = new BalanceSheetDetail()
        {
        };
        return await balanceSheetDetailRepo.CreateAsync(newDetail);
    }

    private static decimal ParseDecimal(string text)
    {
        return string.IsNullOrEmpty(text) ? 0 : decimal.Parse(text);
    }
}