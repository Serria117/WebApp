using System.Diagnostics;
using System.Linq.Dynamic.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.UserService.Dto;
using X.Extensions.PagedList.EF;
using X.PagedList;

namespace WebApp.Services.UserService
{
    public interface IRoleAppService
    {
        Task<RoleDisplayDto> CreateRole(RoleInputDto dto);
        Task<AppResponse> GetAllRoles(PageRequest page);
        Task UpdateRole(int roleId, RoleInputDto dto);
        Task<AppResponse> GetAllPermissionsInRole(int roleId);
    }

    public class RoleAppService(
        IConfiguration configuration,
        IAppRepository<User, Guid> userRepository,
        IAppRepository<Role, int> roleRepository,
        IAppRepository<Permission, int> permissionRepository,
        IUserMongoRepository userMongoRepository,
        IHttpContextAccessor http,
        ILogger<RoleAppService> logger,
        IMapper mapper) : IRoleAppService
    {
        public async Task<AppResponse> GetAllRoles(PageRequest page)
        {
            var stopWatch = Stopwatch.StartNew();
            var pagedResult = await roleRepository.Find(
                    condition: r => !r.Deleted && (string.IsNullOrEmpty(page.Keyword) || r.RoleName.Contains(page.Keyword)), 
                    sortBy: page.SortBy, order: page.OrderBy, 
                    include: ["Permissions"])
                .AsSplitQuery()
                .ToPagedListAsync(page.Number, page.Size);
            var dtoResult = mapper.Map<IPagedList<RoleDisplayDto>>(pagedResult);
            logger.LogInformation("Execution time: {time}", stopWatch.ElapsedMilliseconds);
            return new AppResponse
            {
                PageNumber = dtoResult.PageNumber,
                PageSize = dtoResult.PageSize,
                TotalCount = dtoResult.TotalItemCount,
                Data = dtoResult,
                Message = "Ok"
            };
        }

        public async Task<AppResponse> GetAllPermissionsInRole(int roleId)
        {
            var role = await roleRepository.Find(r => r.Id == roleId)
                .Include(r => r.Permissions).FirstOrDefaultAsync();
            return role is null
                ? new AppResponse { Success = false, Message = "Role not found" }
                : AppResponse.SuccessResponse(mapper.Map<RoleDisplayDto>(role));
        }

        public async Task<RoleDisplayDto> CreateRole(RoleInputDto dto)
        {
            var role = mapper.Map<Role>(dto);
            var permissions = await FindAllPermissionById([.. dto.Permissions]);

            if (permissions.Count > 0)
                role.Permissions.UnionWith(permissions);

            await AddUsersToRole(role, dto.User);


            var saved = await roleRepository.CreateAsync(role);
            return mapper.Map<RoleDisplayDto>(saved);
        }

        public async Task UpdateRole(int roleId, RoleInputDto dto)
        {
            var role = await roleRepository.Find(r => r.Id == roleId)
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .AsSplitQuery()
                .FirstOrDefaultAsync();
            if (role is null) throw new Exception("Role id not found");
            mapper.Map(dto, role);
            var newPermissions = await FindAllPermissionById([.. dto.Permissions]);

            var permissionsToRemove = role.Permissions.Except(newPermissions).ToList();
            var permissionsToAdd = newPermissions.Except(role.Permissions).ToList();

            foreach (var permission in permissionsToRemove)
            {
                role.Permissions.Remove(permission);
            }

            foreach (var permission in permissionsToAdd)
            {
                role.Permissions.Add(permission);
            }

            await roleRepository.UpdateAsync(role);
            await UpdatePermissionForAllUsers(roleId);
        }

        private async Task<List<User>> GetUsersHaveRole(int roleId)
        {
            List<User> users = [];
            var role = await roleRepository.FindByIdAsync(roleId);
            if (role is not null)
                users.AddRange(await userRepository.Find(u => u.Roles.Contains(role) && !u.Deleted)
                    .Include(u => u.Roles).ThenInclude(r => r.Permissions)
                    .AsSplitQuery()
                    .ToListAsync());
            return users;
        }

        private async Task UpdatePermissionForAllUsers(int roleId)
        {
            var foundUsers = await GetUsersHaveRole(roleId);
            if (foundUsers.Count == 0) return;
            var userDocs = foundUsers.Select(u => new UserDoc
                {
                    UserId = u.Id.ToString(),
                    Permissions = u.Roles.SelectMany(r => r.Permissions).Select(p => p.PermissionName).ToList()
                })
                .ToList();
            await userMongoRepository.UpdateAllUser(userDocs);
        }


        private async Task<List<Permission>> FindAllPermissionById(List<int> ids)
        {
            return await permissionRepository.Find(p => ids.Contains(p.Id)).ToListAsync();
        }

        private async Task AddUsersToRole(Role role, ICollection<string> usernames)
        {
            var users = await userRepository
                .Find(u => usernames.Contains(u.Username) && !u.Deleted)
                .ToListAsync();
            role.Users.UnionWith(users);
        }
    }
}