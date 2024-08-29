using AutoMapper;
using WebApp.Core.DomainEntities;
using WebApp.Services.CommonService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Services.Mappers;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UserCreateDto, User>()
            .ForMember(u => u.Password, op => op.MapFrom(d => d.Password.PasswordEncode()))
            .ForMember(u => u.Roles, op => op.Ignore())
            ;
        CreateMap<User, UserDisplayDto>()
            .ForMember(d => d.Roles, op => op.MapFrom(u => u.Roles.Select(r => r.RoleName).ToHashSet()))
            ;
    }
}