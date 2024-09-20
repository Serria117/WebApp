using System.ComponentModel.DataAnnotations;

namespace WebApp.Payloads;

public class SyncInvoiceRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    public string From { get; set; } = string.Empty;
    [Required]
    public string To { get; set; } = string.Empty;
}