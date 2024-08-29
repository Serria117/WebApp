namespace WebApp.Services.UserService.Dto;

public class RoleDisplayDto
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ISet<string> Permissions { get; set; } = new HashSet<string>();
}