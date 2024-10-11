using System.Security.Cryptography;
using AutoMapper;
using WebApp.Core.DomainEntities;
using WebApp.Services.CommonService;
using WebApp.Services.OrganizationService.Dto;

namespace WebApp.Services.Mappers;

public class OrgMapper : Profile
{
    public OrgMapper()
    {
        CreateMap<OrganizationInputDto, Organization>()
            .ForMember(des => des.UnsignName, op => op.MapFrom(src => src.FullName.UnSign()))
            .ForMember(des => des.District, op => op.Ignore())
            .ForMember(des => des.TaxOffice, op => op.Ignore())
            .ForAllMembers(op => op.Condition((dto, organization, props) => props != null));

        CreateMap<Organization, OrganizationDisplayDto>();

        //Trim space of all string-type properties when mapping
       // CreateMap<string?, string?>().ConvertUsing(str => str.RemoveSpace()); 
    }
}