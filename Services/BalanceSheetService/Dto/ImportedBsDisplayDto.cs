using WebApp.Core.DomainEntities.Accounting;

namespace WebApp.Services.BalanceSheetService.Dto;

public class ImportedBsDisplayDto
{
    public int Id { get; init; }
    public string? Name { get; set; }
    public int? Year { get; set; }
    public int? BalanceSheetId { get; set; }
    public List<ImportedBsDetailDisplayDto> Details { get; set; } = [];
    public string? OrganizationId { get; set; }
}