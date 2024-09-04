using System.Linq.Dynamic.Core;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Authentication;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.OrganizationService.Dto;
using X.Extensions.PagedList.EF;
using X.PagedList;
using X.PagedList.Extensions;

namespace WebApp.Services.OrganizationService;

public interface IOrganizationAppService
{
    Task<AppResponse> Create(OrganizationInputDto dto);
    Task<AppResponse> Find(PageRequest page);
    Task<AppResponse> GetById(Guid id);
    Task<AppResponse> CheckTaxIdExist(string taxId);
    Task<AppResponse> Update(Guid orgId, OrganizationInputDto updateDto);
}

public class OrganizationAppService(
    IAppRepository<Organization, Guid> orgRepo,
    IMapper mapper,
    IHttpContextAccessor http) : IOrganizationAppService
{
    public async Task<AppResponse> Create(OrganizationInputDto dto)
    {
        if (await TaxIdExist(dto.TaxId))
            return new AppResponse { Success = false, Message = "TaxId has already existed" };
        var newOrg = mapper.Map<Organization>(dto);
        newOrg.CreateBy = http.HttpContext?.User.Identity?.Name;
        var saved = await orgRepo.CreateAsync(newOrg);
        return AppResponse.SuccessResponse(mapper.Map<OrganizationDisplayDto>(saved));
    }

    public async Task<AppResponse> CheckTaxIdExist(string taxId)
    {
        return await TaxIdExist(taxId)
            ? new AppResponse { Success = false, Message = "TaxId has already existed" }
            : new AppResponse { Success = true, Message = "OK" };
    }

    public async Task<AppResponse> Find(PageRequest page)
    {
        var keyword = page.Keyword.RemoveSpace();
        var pagedResult = await orgRepo.Find
            (
                condition: o => !o.Deleted && (string.IsNullOrEmpty(keyword) ||
                                               o.UnsignName.Contains(keyword) ||
                                               o.ShortName == null || o.ShortName.Contains(keyword) ||
                                               o.TaxId.Contains(keyword)),
                sortBy: page.SortBy,
                order: page.OrderBy
            )
            .ToPagedListAsync(page.Number, page.Size);

        var dtoResult = mapper.Map<IPagedList<OrganizationDisplayDto>>(pagedResult);
        return AppResponse.SuccessResponsePaged(dtoResult);
    }

    public async Task<AppResponse> Update(Guid orgId, OrganizationInputDto updateDto)
    {
        
        var foundOrg = await orgRepo.Find(o => o.Id == orgId && !o.Deleted).FirstOrDefaultAsync();
        if (foundOrg is null) 
            return new AppResponse { Success = false, Message = "Organization Id not found" };
        if (await TaxIdExist(updateDto.TaxId) && updateDto.TaxId != foundOrg.TaxId)
        {
            return new AppResponse { Success = false, Message = "The TaxId you enter has already existed" };
        }
        mapper.Map(updateDto, foundOrg);
        var saved = await orgRepo.UpdateAsync(foundOrg);
        return new AppResponse { Success = true, Data = mapper.Map<OrganizationDisplayDto>(saved), Message = "OK" };
    }

    public async Task<AppResponse> GetById(Guid id)
    {
        var org = await orgRepo.Find(o => o.Id == id && !o.Deleted).FirstOrDefaultAsync();
        return org == null
            ? new AppResponse { Success = false, Message = "Id not found" }
            : AppResponse.SuccessResponse(mapper.Map<OrganizationDisplayDto>(org));
    }

    private async Task<bool> TaxIdExist(string taxId)
    {
        return await orgRepo.ExistAsync(o => o.TaxId == taxId);
    }
}