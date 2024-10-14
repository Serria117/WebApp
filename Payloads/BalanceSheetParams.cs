using WebApp.Core.DomainEntities.Accounting;
using WebApp.Services.BalanceSheetService.Dto;

namespace WebApp.Payloads;

public class BalanceSheetParams
{
    public int Year { get; set; }
    public List<ImportedBsDetailCreateDto> Details { get; set; } = [];
}