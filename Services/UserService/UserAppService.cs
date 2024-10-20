using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.UserService.Dto;
using X.Extensions.PagedList.EF;
using X.PagedList;

namespace WebApp.Services.UserService
{
    public interface IUserAppService
    {
        Task<UserDisplayDto> CreateUser(UserInputDto user);
        Task<AuthenticationResponse> Authenticate(UserLoginDto login);
        Task<bool> ExistUsername(string username);
        Task<User?> FindUserByUserName(string username);
        Task<List<Role>> FindAllRoles(ICollection<int> roleIds);
        Task<List<RoleDisplayDto>> FindRolesByUser(Guid userId);
        Task<AppResponse> GetAllUsers(PageRequest page);
        Task UnlockUser(Guid userId);
        Task<AppResponse> ChangeUserRoles(Guid id, List<int> roleIds);
        Task<AppResponse> SelfChangePassword(string oldPassword, string newPassword);
    }

    public class UserAppAppService(IAppRepository<User, Guid> userRepository,
                                   IUserMongoRepository userMongoRepository,
                                   JwtService jwtService,
                                   IConfiguration configuration,
                                   IAppRepository<Role, int> roleRepository,
                                   IUserManager userManager) : IUserAppService
    {
        public async Task<AppResponse> GetAllUsers(PageRequest page)
        {
            var query = userRepository.Find(u => !u.Deleted, "Roles");
            var users = await query
                              .OrderBy(page.Sort)
                              .ToPagedListAsync(page.Number, page.Size);
            return AppResponse.SuccessResponse(users.MapPagedList(x => x.ToDisplayDto()));
        }

        public async Task<UserDisplayDto> CreateUser(UserInputDto userDto)
        {
            if (userDto == null)
                throw new Exception("Invalid user input");

            if (await ExistUsername(userDto.Username))
                throw new Exception("Username has already been taken");

            var user = userDto.ToEntity();
            var roles = await FindAllRoles(userDto.Roles);

            if (roles.Count > 0)
            {
                user.Roles.UnionWith(roles);
            }

            var created = await userRepository.CreateAsync(user);

            await userMongoRepository.InsertUser(await MapToMongo(created));

            return created.ToDisplayDto();
        }

        public async Task<AuthenticationResponse> Authenticate(UserLoginDto login)
        {
            var stopWatch = Stopwatch.StartNew();
            var user = await FindUserByUserName(login.Username);
            var passwordMatch = false;

            Console.WriteLine($"found user in db took: {stopWatch.ElapsedMilliseconds} ms");

            if (user is not null)
            {
                passwordMatch = login.Password.PasswordVerify(user.Password);
            }
            else
            {
                return new AuthenticationResponse
                {
                    Message = "Invalid username or password."
                };
            }

            if (!passwordMatch)
            {
                await LoginFailureHandler(user);
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

            if (user is { LogInFailedCount: > 0, Locked: false }) await ResetAccount(user);
            var issuedAt = DateTime.UtcNow.ToLocalTime();
            var accessToken = jwtService.GenerateToken(user, issuedAt);
            Console.WriteLine($"Generate jwt took: {stopWatch.ElapsedMilliseconds} ms");
            return new AuthenticationResponse
            {
                Success = true,
                Message = "Success",
                Username = user.Username,
                Id = user.Id,
                AccessToken = accessToken,
                IssueAt = issuedAt,
                ExpireAt = jwtService.GetExpiration(accessToken)
            };
        }

        public async Task<AppResponse> SelfChangePassword(string oldPassword, string newPassword)
        {
            var id = userManager.CurrentUserId();
            if (id is null)
                return AppResponse.Error("Unauthorized access");

            var user = await userRepository.FindByIdAsync(Guid.Parse(id));
            if (user is null)
                return AppResponse.Error("User not found");

            var checkOldPassword = oldPassword.PasswordVerify(user.Password);
            if (!checkOldPassword)
                return AppResponse.Error("Invalid old password");

            user.Password = newPassword.BCryptHash();
            await userRepository.UpdateAsync(user);
            return AppResponse.Ok("Password changed successfully");
        }

        public async Task<AppResponse> ChangeUserRoles(Guid id, List<int> roleIds)
        {
            var user = await userRepository.Find(u => u.Id == id, "Roles").FirstOrDefaultAsync();
            if (user is null) return new AppResponse() { Success = false, Message = "User not found" };
            var roles = await roleRepository.Find(r => roleIds.Contains(r.Id)).ToListAsync();
            if (roles.Count == 0) return new AppResponse { Success = false, Message = "Role not found" };
            user.Roles.Clear();
            user.Roles.UnionWith(roles);
            await userRepository.UpdateAsync(user);
            await UpdateUserWithMongo(user);
            return new AppResponse() { Message = "OK" };
        }

        public async Task UnlockUser(Guid userId)
        {
            var user = await userRepository.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user is null) throw new Exception("User not found");
            await ResetAccount(user);
            await userRepository.UpdateAsync(user);
        }


        public async Task<bool> ExistUsername(string username)
        {
            return await userRepository.ExistAsync(user => user.Username == username);
        }

        public async Task<User?> FindUserByUserName(string username)
        {
            return await userRepository.Find(u => u.Username == username && !u.Deleted).FirstOrDefaultAsync();
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
                return roles.MapCollection(r => r.ToDisplayDto()).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task UpdateUserWithMongo(Guid userId)
        {
            var user = await userRepository.FindByIdAsync(userId);
            if (user is not null)
            {
                var userDoc = await MapToMongo(user);
                await userMongoRepository.InsertUser(userDoc);
            }
        }

        private async Task UpdateUserWithMongo(User user)
        {
            var userDoc = await MapToMongo(user);
            await userMongoRepository.UpdateUser(userDoc);
        }

        private async Task<User?> FindUserByUsername(string name)
        {
            return await userRepository
                         .Find(x => x.Username == name && !x.Deleted)
                         .Include(u => u.Roles).ThenInclude(r => r.Permissions)
                         .FirstOrDefaultAsync();
        }

        private async Task<List<string>> GetUserPermissions(Guid id)
        {
            return await userRepository.Find(u => u.Id == id && !u.Deleted)
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
                Permissions = (await GetUserPermissions(user.Id)).ToHashSet(),
            };
        }


        private async Task LoginFailureHandler(User user)
        {
            user.LogInFailedCount += 1; // count login attempt
            //Lock account if attempt reached limit
            if (user.LogInFailedCount == int.Parse(configuration["SecureLogin:FailedCountLimit"]!))
            {
                user.Locked = true;
            }

            await userRepository.UpdateAsync(user);
        }

        private async Task ResetAccount(User user)
        {
            user.LogInFailedCount = 0;
            user.Locked = false;
            await userRepository.UpdateAsync(user);
        }
    }
}