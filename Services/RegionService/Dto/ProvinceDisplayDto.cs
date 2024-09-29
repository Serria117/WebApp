namespace WebApp.Services.RegionService.Dto;

public class ProvinceDisplayDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AlterName { get; set; }
    public HashSet<DistrictDisplayDto> Districts { get; set; } = [];
    public HashSet<TaxOfficeDisplayDto> TaxOffices { get; set; } = [];
}