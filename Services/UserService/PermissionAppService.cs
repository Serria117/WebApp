using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Repositories;

namespace WebApp.Services.UserService
{
    public interface IPermissionAppService
    {
        Task<List<string>> GetPermission(Guid userId);
    }

    public class PermissionAppService(IAppRepository<User, Guid> userRepo) : IPermissionAppService
    {
        public async Task<List<string>> GetPermission(Guid userId)
        {
            var user = await userRepo.Find(x => x.Id == userId)
                .Include(x => x.Roles)
                .ThenInclude(r => r.Permissions)
                .FirstOrDefaultAsync();
            return user is null
                ? throw new Exception("Invalid userId")
                : user.Roles.Where(r => !r.Deleted)
                .SelectMany(r => r.Permissions)
                .Where(p => !p.Deleted)
                .Select(p => p.PermissionName)
                .Distinct()
                .ToList();
        }
    }
}
