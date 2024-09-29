namespace WebApp.Payloads;

public class InvoiceParams
{
    public string? SellerKeyword { get; set; }
    public int? InvoiceNumber { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    
    public InvoiceParams Valid()
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