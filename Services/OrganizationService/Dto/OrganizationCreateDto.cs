using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.OrganizationService.Dto;

public class OrganizationCreateDto
{
    [Length(3, 500)] [Required]
    public string FullName { get; set; } = string.Empty;

    [Length(3, 50)] 
    public string? ShortName { get; set; }

    [Length(10, 20)] [Required]
    public string TaxId { get; set; } = string.Empty;
    
    public List<string> Emails { get; set; } = [];
    public List<string> Phones { get; set; } = [];
    public string? TaxIdPwd { get; set; }
    public string? InvoicePwd { get; set; }
    
    [MaxLength(1000)]
    public string? Address { get; set; }

    [MaxLength(1000)]
    public string? ContactAddress { get; set; }
}