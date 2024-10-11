namespace WebApp.Services.UserService.Dto;

public class UserDisplayDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public HashSet<RoleDisplayDto> Roles { get; set; } = [];
}
