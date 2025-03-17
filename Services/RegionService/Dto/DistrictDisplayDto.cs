namespace WebApp.Services.RegionService.Dto;

public class DistrictDisplayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AlterName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
}