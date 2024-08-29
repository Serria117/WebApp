using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.UserService.Dto;

public class UserCreateDto
{
    [MinLength(3), MaxLength(255)]
    public string Username { get; set; } = string.Empty;
    [MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
    public ISet<int> Roles { get; set; } = new HashSet<int>();
}