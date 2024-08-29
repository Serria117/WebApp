namespace WebApp.Payloads;

public class AuthenticationResponse
{
    public bool Success { get; set; } = false;
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? Username { get; set; }
    public DateTime? IssueAt { get; set; }
    public DateTime? ExpireAt { get; set; }
}