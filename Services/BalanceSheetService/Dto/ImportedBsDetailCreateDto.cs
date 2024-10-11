using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.BalanceSheetService.Dto;

public class ImportedBsDetailCreateDto
{
    [MaxLength(10)]
    public string? Account { get; set; }
    
    public decimal OpenCredit { get; set; }

    public decimal OpenDebit { get; set; }
    
    public decimal AriseCredit { get; set; }

    public decimal AriseDebit { get; set; }
    
    public decimal CloseCredit { get; set; }

    public decimal CloseDebit { get; set; }

    public bool? IsValid { get; set; }

    [MaxLength(255)]
    public string? Note { get; set; }
}