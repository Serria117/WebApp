using AutoMapper;
using WebApp.Core.DomainEntities;
using WebApp.Services.UserService.Dto;

namespace WebApp.Services.Mappers;

public class RoleMapper : Profile
{
    public RoleMapper()
    {
        CreateMap<Role, RoleDisplayDto>()
            .ForMember(
                d => d.Permissions,
                op => op.MapFrom(r => r.Permissions.Select(p => p.PermissionName).ToHashSet())
            )
            ;
    }
}