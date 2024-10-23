namespace WebApp.Payloads;

public class UploadBalanceSheet
{
    public Guid OrgId { get; set; }
    public int Year { get; set; }
    public IFormFile File { get; set; }
}