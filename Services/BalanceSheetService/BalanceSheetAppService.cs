﻿using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Spire.Xls;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Services.Mappers;

namespace WebApp.Services.BalanceSheetService;

public interface IBalanceSheetAppService
{
    Task<AppResponse> ProcessBalanceSheet(Guid orgId, int year, IFormFile file);
    Task<AppResponse> CreateImportedBalanceSheet(Guid orgId, BalanceSheetParams input);
    Task<AppResponse> GetImportedBalanceSheetsByOrg(Guid orgId);
    Task<AppResponse> GetImportedBalanceSheets(int id);
    Task<AppResponse> DeleteImportedBalanceSheet(int id);
    Task<AppResponse> HardDeleteImportedBalanceSheet(int id);
    Task<AppResponse> GetAccountTemplate();
    Task<AppResponse> CreateAccountTemplate(AccountCreateDto input);
}

public class BalanceSheetAppService(AppDbContext db,
                                    IUnitOfWork transaction,
                                    IAppRepository<Account, int> accountRepo,
                                    IAppRepository<BalanceSheet, int> balanceSheetRepo,
                                    IAppRepository<BalanceSheetDetail, int> balanceSheetDetailRepo,
                                    IAppRepository<ImportedBalanceSheet, int> importedBsRepo,
                                    IAppRepository<ImportedBalanceSheetDetail, int> importedBsDetailRepo,
                                    IAppRepository<Organization, Guid> orgRepo,
                                    ILogger<BalanceSheetAppService> logger) : IBalanceSheetAppService
{
    /// <summary>
    /// Creates an imported balance sheet and its details from the provided input.
    /// </summary>
    /// <param name="orgId">The organization id.</param>
    /// <param name="input">The input containing the year and details of the balance sheet.</param>
    /// <returns>
    /// A response indicating success or failure, with the id of the created balance sheet and
    /// its validation result if successful, or an error message if failed.
    /// </returns>
    public async Task<AppResponse> CreateImportedBalanceSheet(Guid orgId, BalanceSheetParams input)
    {
        if (!await orgRepo.ExistAsync(o => o.Id == orgId))
            return AppResponse.Error("org id cannot be found");

        if (input?.Details == null || !input.Details.Any())
            return AppResponse.Error("Details cannot be null or empty");

        var balanceSheet = new ImportedBalanceSheet
        {
            Organization = orgRepo.Attach(orgId),
            Year = input.Year,
            Details = input.Details.MapCollection(x => x.ToEntity()).ToHashSet(),
        };

        try
        {
            CalculateBalanceSheetTotal(balanceSheet);
        }
        catch (Exception ex)
        {
            logger.LogError("Error calculating balance sheet total: {message}", ex.Message);
            return AppResponse.Error("Failed to calculate balance sheet total");
        }

        try
        {
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
        catch (Exception ex)
        {
            logger.LogError("Error saving balance sheet: {message}", ex.Message);
            return AppResponse.Error("Failed to save balance sheet");
        }
    }

    public async Task<AppResponse> GetImportedBalanceSheetsByOrg(Guid orgId)
    {
        var result = await importedBsRepo.Find(x => x.Organization != null
                                                    && x.Organization.Id == orgId
                                                    && !x.Deleted,
                                               sortBy: nameof(ImportedBalanceSheet.CreateAt),
                                               order: SortOrder.DESC)
                                         .ToListAsync();
        return result.IsNullOrEmpty()
            ? AppResponse.Error(ResponseMessage.NotFound)
            : AppResponse.SuccessResponse(result.MapCollection(x => x.ToDisplayDto()));
    }

    public async Task<AppResponse> GetImportedBalanceSheets(int id)
    {
        var result = await importedBsRepo.Find(filter: x => x.Id == id && !x.Deleted,
                                               include: [nameof(ImportedBalanceSheet.Details)])
                                         .FirstOrDefaultAsync();

        if (result is null) return AppResponse.Error(ResponseMessage.NotFound);

        return AppResponse.SuccessResponse(result.ToDisplayDto());
    }

    public async Task<AppResponse> DeleteImportedBalanceSheet(int id)
    {
        await transaction.BeginTransactionAsync();
        var detailIds = await importedBsDetailRepo.Find(x => x.ImportedBalanceSheet.Id == id)
                                                  .Select(x => x.Id)
                                                  .ToArrayAsync();
        await importedBsRepo.SoftDeleteAsync(id);
        await importedBsDetailRepo.SoftDeleteManyAsync(detailIds);
        await transaction.CommitAsync();
        return AppResponse.Ok();
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
        await db.ExecuteInTransaction(async () =>
        {
            await db.ImportedBalanceSheets
                    .Where(x => x.Id == id)
                    .ExecuteDeleteAsync();
            await db.ImportedBalanceSheetDetails
                    .Where(x => x.ImportedBalanceSheet.Id == id)
                    .ExecuteDeleteAsync();
        });

        return AppResponse.Ok();
    }

    #region Account Template

    public async Task<AppResponse> GetAccountTemplate()
    {
        var result = await db.Accounts.Where(x => !x.Deleted)
                             .OrderBy(x => x.AccountNumber)
                             .AsNoTracking()
                             .ToListAsync();
        return AppResponse.SuccessResponse(result);
    }

    public async Task<AppResponse> CreateAccountTemplate(AccountCreateDto input)
    {
        var account = input.ToEntity();
        await accountRepo.CreateAsync(account);
        return AppResponse.Ok();
    }
    
    #endregion
    

    #region PRIVATE METHODS

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
        var cell111 = worksheet.FindAll("111", FindType.Text, ExcelFindOptions.MatchEntireCellContent)
                               .FirstOrDefault();
        var startRow = cell111?.Row ?? 3;

        for (var i = startRow; i <= worksheet.Rows.Length; i++)
        {
            var detail = new ImportedBalanceSheetDetail
            {
                Account = worksheet.Range[i, 1].Value,
                OpenCredit = ParseDecimal(worksheet.Range[i, 3].Value),
                OpenDebit = ParseDecimal(worksheet.Range[i, 4].Value),
                AriseCredit = ParseDecimal(worksheet.Range[i, 5].Value),
                AriseDebit = ParseDecimal(worksheet.Range[i, 6].Value),
                CloseCredit = ParseDecimal(worksheet.Range[i, 7].Value),
                CloseDebit = ParseDecimal(worksheet.Range[i, 8].Value),
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

    #endregion
}