namespace WebApp.Payloads;

public class DateRange
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public string GetFromDateString()
    {
        return FromDate.ToString("dd/MM/yyyy");
    }

    public string GetToDateString()
    {
        return ToDate.ToString("dd/MM/yyyy");
    }
    
    public override string ToString()
    {
        return $"From: {FromDate:dd/MM/yyyy} - To: {ToDate:dd/MM/yyyy}";
    }
}