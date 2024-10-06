using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Spire.Xls;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.UserService;

namespace WebApp.Services.BalanceSheetService;

public interface IBalanceSheetAppService
{
    Task<AppResponse> ProcessBalanceSheet(Guid orgId, int year, IFormFile file);
}

public class BalanceSheetAppService(IAppRepository<Account, int> accountRepo,
                                    IAppRepository<BalanceSheet, int> balanceSheetRepo,
                                    IAppRepository<BalanceSheetDetail, int> balanceSheetDetailRepo,
                                    IAppRepository<ImportedBalanceSheet, int> balanceSheetImportedRepo,
                                    IAppRepository<ImportedBalanceSheetDetail, int> importedBalanceSheetRepo,
                                    ILogger<BalanceSheetAppService> logger, 
                                    IUserManager userManager) : AppServiceBase(userManager), IBalanceSheetAppService
{
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
                Account = int.Parse(worksheet.Range[i, 1].Value),
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
        var accounts = await accountRepo.FindAndSort(x => !x.Deleted, [], [nameof(Account.Id)])
                                        .Select(x => x.Id)
                                        .ToListAsync();
        
        if (!accounts.Contains(imported.Account))
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