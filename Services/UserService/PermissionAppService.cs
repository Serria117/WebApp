using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Mongo.MongoRepositories;
using WebApp.Repositories;

namespace WebApp.Services.UserService
{
    public interface IPermissionAppService
    {
        Task<List<string>> GetPermissions(Guid userId);
        Task<List<string>> GetPermissionsFromMongo(Guid userId);
    }

    public class PermissionAppService(IAppRepository<User, Guid> userRepo, IMongoRepository mongoRepo) : IPermissionAppService
    {
        public async Task<List<string>> GetPermissions(Guid userId)
        {
            return await userRepo.GetQueryable().Where(u => u.Id == userId)
                .Include(u => u.Roles).ThenInclude(r => r.Permissions)
                .SelectMany(u => u.Roles)
                .SelectMany(r => r.Permissions).Select(p => p.PermissionName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetPermissionsFromMongo(Guid userId)
        {
            return [.. (await mongoRepo.GetUser(userId)).Permissions];
        }
    }
}
