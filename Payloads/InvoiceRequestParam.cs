using WebApp.Enums;

namespace WebApp.Payloads;

/// <summary>
/// Wraper for query invoice from mongodb
/// </summary>
public class InvoiceRequestParam
{
    public string? NameKeyword { get; set; }
    public int? InvoiceNumber { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    
    public InvoiceType? InvoiceType { get; set; }

    public InvoiceStatus? Status { get; set; }
    
    public bool? Risk { get; set; }
    
    public InvoiceRequestParam Valid()
    {
        if (Size is <= 0 or > 1000 or null)
        {
            Size = 10;
        }

        if (Page is <= 0 or > 1000 or null)
        {
            Page = 1;
        }
        return this;
    }
}

