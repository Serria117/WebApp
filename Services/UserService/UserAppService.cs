﻿using System.Diagnostics;
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
using X.Extensions.PagedList.EF;

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
        Task<AppResponse> ChangeUserRoles(Guid id, List<int> roleIds);
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
            var query = userRepository.Find(u => !u.Deleted, "Roles");
            var users = await query
                .OrderBy(page.Sort)
                .ToPagedListAsync(page.Number, page.Size);
            return new AppResponse
            {
                Data = users.Select(mapper.Map<UserDisplayDto>).ToList(),
                PageNumber = page.Number,
                PageSize = page.Size,
                TotalCount = users.TotalItemCount
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
            var stopWatch = Stopwatch.StartNew();
            var user = await FindUserByUserName(login.Username);
            bool passwordMatch = false;
            /*user = new User
            {
                Id = Guid.Parse("30a34f86-46ca-4370-3e6c-08dcc8a7abfb"),
                Username = "admin",
                Password = "$2a$12$CEvIlWrwzU2LjabBFJRoY.9lpXN8EULsznFlFDKdm3W07KMh5y6FG"
            };*/
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
                AccessToken = accessToken,
                IssueAt = issuedAt,
                ExpireAt = jwtService.GetExpiration(accessToken)
            };
        }

        public async Task<AppResponse> ChangeUserRoles(Guid id, List<int> roleIds)
        {
            var user = await userRepository.Find(u => u.Id == id, "Roles").FirstOrDefaultAsync();
            if (user is null) return new AppResponse() { Success = false, Message = "User not found" };
            var roles = await roleRepository.Find(r => roleIds.Contains(r.Id)).ToListAsync();
            if (roles.Count == 0) return new AppResponse() { Success = false, Message = "Role not found" };
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
                return roles.Select(mapper.Map<RoleDisplayDto>).ToList();
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
                await mongoRepository.InsertUser(userDoc);
            }
        }

        private async Task UpdateUserWithMongo(User user)
        {
            var userDoc = await MapToMongo(user);
            await mongoRepository.UpdateUser(userDoc);
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
                Permissions = await GetUserPermissions(user.Id)
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