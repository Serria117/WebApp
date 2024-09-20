namespace WebApp.Services.RestService.Dto;

public class TokenModel
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public string Token { get; set; } = string.Empty;
}
