using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.OrganizationService.Dto;

public class OrganizationInputDto
{
    [Length(3, 500)] [Required(ErrorMessage = "Name is required")]
    public string FullName { get; set; } = string.Empty;

    [Length(3, 50)]
    public string? ShortName { get; set; }

    [Length(10, 20)] [Required(ErrorMessage = "TaxId is required")]
    public string TaxId { get; set; } = string.Empty;

    public List<string> Emails { get; set; } = [];
    public List<string> Phones { get; set; } = [];
    public string? TaxIdPwd { get; set; }
    public string? InvoicePwd { get; set; }

    [MaxLength(50)] [MinLength(3)]
    public string? PinCode { get; set; }

    [MaxLength(1000)]
    public string? Address { get; set; }

    [MaxLength(1000)]
    public string? ContactAddress { get; set; }

    [Required(ErrorMessage = "District is required")]
    public int? DistrictId { get; set; }

    [Required(ErrorMessage = "TaxOffice is required")]
    public int? TaxOfficeId { get; set; }
    
    [RegularExpression(@"^\d{2}/\d{2}$", ErrorMessage = "Please enter valid date format: dd/mm for FiscalYearFirstDate")]
    public string? FiscalYearFirstDate { get; set; } = "01/01";
    //public DateTime LastUpdateAt { get; set; } = DateTime.UtcNow.ToLocalTime();
}