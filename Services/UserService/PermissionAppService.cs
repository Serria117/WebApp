using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.UserService.Dto;

namespace WebApp.Services.UserService
{
    public interface IPermissionAppService
    {
        Task<List<string>> GetPermissions(Guid userId);
        Task<List<string>> GetPermissionsFromMongo(Guid userId);
        Task<AppResponse> GetAllPermissionsInSystem();
    }

    public class PermissionAppService(IAppRepository<User, Guid> userRepo, 
        IAppRepository<Permission, int> permissionRepo,
        IMapper mapper,
        IMongoRepository mongoRepo) : IPermissionAppService
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

        public async Task<AppResponse> GetAllPermissionsInSystem()
        {
            var permissions = await permissionRepo.Find(p => !p.Deleted).ToListAsync();
            //permissions.Select(mapper.Map<PermissionDisplayDto>)
            return AppResponse.SuccessResponse(mapper.Map<List<PermissionDisplayDto>>(permissions));
        }
    }
}
