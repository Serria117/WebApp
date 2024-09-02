using AutoMapper;
using WebApp.Core.DomainEntities;
using WebApp.Services.OrganizationService.Dto;

namespace WebApp.Services.Mappers;

public class OrgMapper : Profile
{
    public OrgMapper()
    {
        CreateMap<OrganizationCreateDto, Organization>()
            .ForAllMembers(op => op.Condition((dto, organization, props) => props != null));

        CreateMap<Organization, OrganizationDisplayDto>();
    }
}