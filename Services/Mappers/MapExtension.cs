using WebApp.Core.DomainEntities;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.OrganizationService.Dto;
using WebApp.Services.RegionService.Dto;
using X.PagedList;

namespace WebApp.Services.Mappers;

public static class MapExtension
{
    public static OrganizationDisplayDto ToDisplayDto(this Organization o)
    {
        return new OrganizationDisplayDto
        {
            Id = o.Id,
            FullName = o.FullName,
            Address = o.Address,
            District = o.District is null
                ? null
                : new DistrictDisplayDto
                {
                    Id = o.District!.Id,
                    Name = o.District.Name,
                    AlterName = o.District.AlterName,
                    Province = o.District.Province.Name,
                },
            Emails = o.Emails,
            Phones = o.Phones,
            ContactAddress = o.ContactAddress,
            TaxId = o.TaxId,
            TaxOffice = o.TaxOffice is null
                ? null
                : new TaxOfficeDisplayDto
                {
                    Id = o.TaxOffice.Id,
                    FullName = o.TaxOffice.FullName,
                    ShortName = o.TaxOffice.ShortName,
                    Code = o.TaxOffice.Code,
                    ParentId = o.TaxOffice.ParentId,
                },
            ShortName = o.ShortName,
            InvoicePwd = o.InvoicePwd,
            PinCode = o.PinCode,
            TaxIdPwd = o.TaxIdPwd,
            CreateAt = o.CreateAt,
            CreateBy = o.CreateBy,
            LastUpdateAt = o.LastUpdateAt,
        };
    }

    public static IPagedList<OrganizationDisplayDto> ToDisplayDto(this IPagedList<Organization> orgs)
    {
        var dtoList = orgs.Select(o => o.ToDisplayDto()).ToList();
        return new StaticPagedList<OrganizationDisplayDto>(subset: dtoList, 
                                                           pageNumber: orgs.PageNumber, 
                                                           pageSize: orgs.PageSize, 
                                                           totalItemCount: orgs.TotalItemCount);
    }

    public static Organization ToEntity(this OrganizationInputDto i)
    {
        return new Organization
        {
            FullName = i.FullName.RemoveSpace()!,
            ShortName = i.ShortName.RemoveSpace()!,
            Address = i.Address.RemoveSpace()!,
            ContactAddress = i.ContactAddress.RemoveSpace()!,
            Emails = i.Emails.Select(x => x.RemoveSpace()!).ToList(),
            Phones = i.Phones.Select(x => x.RemoveSpace()!).ToList(),
            InvoicePwd = i.InvoicePwd,
            PinCode = i.PinCode,
            TaxId = i.TaxId.RemoveSpace()!,
            UnsignName = i.FullName.RemoveSpace()!.UnSign(),
            TaxIdPwd = i.TaxIdPwd,
        };
    }
}