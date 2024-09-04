using AutoMapper;
using WebApp.Core.DomainEntities;
using WebApp.Services.CommonService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Services.Mappers;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UserInputDto, User>()
            .ForMember(u => u.Password, op => op.MapFrom(d => d.Password.BCryptHash()))
            .ForMember(u => u.Roles, op => op.Ignore())
            .ForAllMembers(op => op.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<User, UserDisplayDto>()
            .ForMember(d => d.Roles, op => op.MapFrom(u => u.Roles.Select(r => r.RoleName).ToHashSet()));
    }
}