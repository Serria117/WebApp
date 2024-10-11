using AutoMapper;
using WebApp.Core.DomainEntities;
using WebApp.Services.RegionService.Dto;
using ZstdSharp.Unsafe;

namespace WebApp.Services.Mappers;

public class RegionMapper : Profile
{
    public RegionMapper()
    {
        CreateMap<TaxOffice, TaxOfficeDisplayDto>()
            .ForMember(dest => dest.Province, opt => opt.MapFrom(scr => scr.Province.Name))
            .ForAllMembers(op => op.Condition((_, _, prop) => prop is not null));
        
        CreateMap<TaxOfficeCreateDto, TaxOffice>()
            .ForMember(dest => dest.Province, opt => opt.Ignore());

        CreateMap<District, DistrictDisplayDto>()
            .ForAllMembers(op => op.Condition((scr, dest, props) => props is not null));
        
        CreateMap<DistrictCreateDto, District>()
            .ForMember(dest => dest.Province, opt => opt.Ignore())
            .ForAllMembers(op => op.Condition((scr, dest, props) => props is not null));

        CreateMap<Province, ProvinceDisplayDto>();
        
        CreateMap<ProvinceCreateDto, Province>();
    }
}