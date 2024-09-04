using System.Security.Cryptography;
using AutoMapper;
using WebApp.Core.DomainEntities;
using WebApp.Services.OrganizationService.Dto;

namespace WebApp.Services.Mappers;

public class OrgMapper : Profile
{
    public OrgMapper()
    {
        CreateMap<OrganizationInputDto, Organization>()
            .ForMember(des => des.LastUpdateAt, op => op.MapFrom(_ => DateTime.UtcNow.ToLocalTime()))
            .ForAllMembers(op => op.Condition((dto, organization, props) => props != null));

        CreateMap<Organization, OrganizationDisplayDto>();
    }
}