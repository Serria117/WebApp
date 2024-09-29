using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.RegionService.Dto;

public class TaxOfficeCreateDto
{
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ShortName { get; set; }

    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    public int ProvinceId { get; set; }
    
    public int? ParentId { get; set; }
}