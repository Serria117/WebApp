namespace WebApp.Services.UserService.Dto;

public class PermissionDisplayDto
{
    public int Id { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string? Description { get; set; }
}