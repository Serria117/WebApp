namespace WebApp.Payloads;

public class InvoiceParams
{
    public string? SellerKeyword { get; set; }
    public int? InvoiceNumber { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    
    private int _page;
    public int Page
    {
        get => _page;
        set
        {
            if (value is <= 0)
            {
                _page = 1;
            }

            _page = value;
        }
    }

    private int _size;
    public int Size
    {
        get => _size;
        set
        {
            _size = value switch
            {
                <= 0 => 10,
                > 50 => 50,
                _ => _size
            };
            _size = value;
        }
    }
}