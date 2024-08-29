namespace WebApp.Services.UserService.Dto
{
    public class RoleCreateDto
    {
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ISet<int> Permissions { get; set; } = new HashSet<int>();
        public ISet<string> User { get; set; } = new HashSet<string>();
    }
}
