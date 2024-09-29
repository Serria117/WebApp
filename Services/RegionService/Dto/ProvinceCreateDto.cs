using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.RegionService.Dto;

public class ProvinceCreateDto
{
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? AlterName { get; set; }
}