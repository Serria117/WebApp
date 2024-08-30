using System.Linq.Dynamic.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Mongo.MongoCollections;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Services.UserService
{
    public interface IUserService
    {
        Task<UserDisplayDto> CreateUser(UserCreateDto user);
        Task<AuthenticationResponse> Authenticate(UserLoginDto login);
        Task<bool> ExistUsername(string username);
        Task<User?> FindUserByUserName(string username);
        Task<List<Role>> FindAllRoles(ICollection<int> roleIds);
        Task<List<RoleDisplayDto>> FindRolesByUser(Guid userId);
        Task<AppResponse> GetAllUsers(PageRequest page);
        Task UnlockUser(Guid userId);
    }

    public class UserAppService(
        IMapper mapper,
        IAppRepository<User, Guid> userRepository,
        IMongoRepository mongoRepository,
        JwtService jwtService,
        IConfiguration configuration,
        IAppRepository<Role, int> roleRepository) : IUserService
    {
        public async Task<AppResponse> GetAllUsers(PageRequest page)
        {
            var query = userRepository.Find(u => u.Deleted == false, "Roles");
            var users = await query.OrderBy($"{page.SortBy} {page.OrderBy}")
                .Skip(page.Skip)
                .Take(page.Take)
                .Select(u => mapper.Map<UserDisplayDto>(u))
                .ToListAsync();
            var count = await query.CountAsync();
            return new AppResponse
            {
                Data = users,
                PageNumber = page.Page,
                PageSize = page.Size,
                TotalCount = count
            };
        }

        public async Task<UserDisplayDto> CreateUser(UserCreateDto userDto)
        {
            if (userDto == null)
                throw new Exception("Invalid user input");

            if (await ExistUsername(userDto.Username))
                throw new Exception("Username has already been taken");

            var user = mapper.Map<User>(userDto);
            var roles = await FindAllRoles(userDto.Roles);

            if (roles.Count > 0)
            {
                user.Roles.UnionWith(roles);
            }
            var created = await userRepository.CreateAsync(user);

            await mongoRepository.InsertUser(await MapToMongo(created));

            return mapper.Map<UserDisplayDto>(created);
        }

        public async Task<AuthenticationResponse> Authenticate(UserLoginDto login)
        {
            var user = await FindUserByUserName(login.Username);
            if (user is null || !login.Password.PasswordVerify(user.Password))
            {
                if (user is null || user.Deleted)
                {
                    return new AuthenticationResponse
                    {
                        Message = "Invalid username or password."
                    };
                }

                if (user.Locked)
                {
                    return new AuthenticationResponse
                    {
                        Message = "Your account has been locked."
                    };
                }

                await LoginCount(user);
                return new AuthenticationResponse
                {
                    Message = "Invalid username or password."
                };
            }

            await ResetCount(user);
            var issuedAt = DateTime.Now;
            var accessToken = jwtService.GenerateToken(user, issuedAt);
            return new AuthenticationResponse
            {
                Success = true,
                Message = "Success",
                Username = user.Username,
                AccessToken = accessToken,
                IssueAt = issuedAt,
                ExpireAt = jwtService.GetExpiration(accessToken)
            };
        }

        public async Task UnlockUser(Guid userId)
        {
            var user = await userRepository.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user is null) throw new Exception("User not found");
            await ResetCount(user);
            await userRepository.UpdateAsync(user);
        }


        public async Task<bool> ExistUsername(string username)
        {
            return await userRepository.ExistAsync(user => user.Username == username);
        }

        public async Task<User?> FindUserByUserName(string username)
        {
            return await userRepository.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<List<Role>> FindAllRoles(ICollection<int> roleIds)
        {
            return await roleRepository.Find(r => roleIds.Contains(r.Id)).ToListAsync();
        }

        public async Task<List<RoleDisplayDto>> FindRolesByUser(Guid userId)
        {
            try
            {
                var user = await userRepository.FindByIdAsync(userId);
                if (user is null) throw new Exception("User not found");
                var roles = await roleRepository.Find(r => r.Users.Contains(user))
                    .ToListAsync();
                return roles.Select(mapper.Map<RoleDisplayDto>).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<User?> FindUserByUsername(string name)
        {
            return await userRepository
                .Find(x => x.Username == name && !x.Deleted)
                .FirstOrDefaultAsync();
        }

        private async Task<List<string>> GetUserPermissions(User user)
        {
            return await userRepository.GetQueryable().Where(u => u.Id == user.Id)
                .Include(u => u.Roles).ThenInclude(r => r.Permissions)
                .SelectMany(u => u.Roles)
                .SelectMany(r => r.Permissions).Select(p => p.PermissionName)
                .Distinct()
                .ToListAsync();
        }

        private async Task<UserDoc> MapToMongo(User user)
        {
            return new UserDoc
            {
                UserId = user.Id.ToString(),
                Permissions = await GetUserPermissions(user)
            };
        }


        private async Task LoginCount(User user)
        {
            user.LogInFailedCount += 1;
            if (user.LogInFailedCount == int.Parse(configuration["SecureLogin:FailedCountLimit"]!))
            {
                user.Locked = true;
            }

            await userRepository.UpdateAsync(user);
        }

        private async Task ResetCount(User user)
        {
            user.LogInFailedCount = 0;
            await userRepository.UpdateAsync(user);
        }
    }
}