namespace WebApp.Services.RegionService.Dto;

public class TaxOfficeDisplayDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Province { get; set; }
    public int? ParentId { get; set; }
}