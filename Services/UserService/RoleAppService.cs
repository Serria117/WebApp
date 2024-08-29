using System.Linq.Dynamic.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.UserService.Dto;

namespace WebApp.Services.UserService
{
    public interface IRoleAppService
    {
        Task<RoleDisplayDto> CreateRole(RoleCreateDto dto);
        Task<AppResponse> GetAllRoles(PageRequest paging);
        Task UpdateRole(int roleId, RoleCreateDto dto);
    }

    public class RoleAppService(
            IConfiguration configuration,
            IAppRepository<User, Guid> userRepository,
            IAppRepository<Role, int> roleRepository,
            IAppRepository<Permission, int> permissionRepository,
            IHttpContextAccessor http,
            IMapper mapper) : IRoleAppService
    {
        public async Task<AppResponse> GetAllRoles(PageRequest paging)
        {
            var result = (await roleRepository.FindAllAsync(paging.Skip, paging.Take, $"{paging.SortBy} {paging.OrderBy}", "Permissions"))
                .Select(mapper.Map<RoleDisplayDto>)
                .ToList();
            var count = await roleRepository.CountAsync();
            return new AppResponse
            {
                PageNumber = paging.Page,
                PageSize = paging.Size,
                TotalCount = count,
                Data = result,
                Message = "Ok"
            };
        }

        public async Task<RoleDisplayDto> CreateRole(RoleCreateDto dto)
        {
            var role = mapper.Map<Role>(dto);
            var permissions = await FindAllPermissionById([.. dto.Permissions]);

            if (permissions.Count > 0)
                role.Permissions.UnionWith(permissions);

            if (dto.User is not null)
                await AddUsersToRole(role, dto.User);


            var saved = await roleRepository.CreateAsync(role);
            return mapper.Map<RoleDisplayDto>(saved);
        }

        public async Task UpdateRole(int roleId, RoleCreateDto dto)
        {
            var role = await roleRepository.FindByIdAsync(roleId);
            if (role is null) throw new Exception("Role id not found");
            mapper.Map(dto, role);
            await roleRepository.UpdateAsync(role);
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
