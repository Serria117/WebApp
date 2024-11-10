using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace WebApp.Services.BalanceSheetService.Dto;

public class AccountCreateDto
{
    [Required]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public int? Parent { get; set; }

    public int Grade { get; set; }

    public string? B01TS { get; set; }

    public string? B01NV { get; set; }

    public string? B02 { get; set; }
}