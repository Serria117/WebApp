using System.Security.Cryptography;
using AutoMapper;
using WebApp.Core.DomainEntities;
using WebApp.Services.UserService.Dto;

namespace WebApp.Services.Mappers;

public class RoleMapper : Profile
{
    public RoleMapper()
    {
        CreateMap<Role, RoleDisplayDto>()
            .ForMember(des => des.Permissions, op => op.MapFrom(r => r.Permissions.Select(p => p.PermissionName).ToHashSet()))
            .ForMember(des => des.Users, op => op.MapFrom(r => r.Users.Select(u => u.Username).ToHashSet()));

        CreateMap<RoleInputDto, Role>()
            .ForMember(des => des.Permissions, op => op.Ignore())
            .ForAllMembers(opts => opts.Condition((src, des, srcMember) => srcMember != null));

        CreateMap<Permission, PermissionDisplayDto>()
            .ForAllMembers(op => op.Condition((src, des, srcMember) => srcMember != null));
        
       
    }
}