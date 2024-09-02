using System.Linq.Dynamic.Core;
using AutoMapper;
using WebApp.Authentication;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.OrganizationService.Dto;
using X.Extensions.PagedList.EF;
using X.PagedList.Extensions;

namespace WebApp.Services.OrganizationService;

public interface IOrganizationAppService
{
    Task<AppResponse> Create(OrganizationCreateDto dto);
    Task<AppResponse> Find(PageRequest page);
    Task<AppResponse> GetById(Guid id);
    Task<AppResponse> CheckTaxIdExist(string taxId);
}

public class OrganizationAppService(
    IAppRepository<Organization, Guid> orgRepo,
    IMapper mapper,
    IHttpContextAccessor http) : IOrganizationAppService
{
    public async Task<AppResponse> Create(OrganizationCreateDto dto)
    {
        if (await TaxIdExist(dto.TaxId))
            return new AppResponse { Success = false, Message = "TaxId has already existed" };
        var newOrg = mapper.Map<Organization>(dto);
        var saved = await orgRepo.CreateAsync(newOrg);
        return AppResponse.SuccessResponse(mapper.Map<OrganizationDisplayDto>(saved));
    }

    public async Task<AppResponse> CheckTaxIdExist(string taxId)
    {
        return await TaxIdExist(taxId)
            ? new AppResponse { Success = false, Message = "TaxId has already existed" }
            : new AppResponse { Success = true, Message = "OK"};
    }
    
    public async Task<AppResponse> Find(PageRequest page)
    {
        var query = orgRepo.Find(x => !x.Deleted);
        if (!string.IsNullOrEmpty(page.Keyword))
        {
            query = query.Where(x =>
                x.FullName.Contains(page.Keyword)
                || x.TaxId.Contains(page.Keyword)
                || (x.ShortName != null && x.ShortName.Contains(page.Keyword))
            );
        }

        var result = await query
            .OrderBy(page.Sort)
            .ToPagedListAsync(page.Number, page.Size);

        return AppResponse.SuccessResponsePaged(
            result.Select(mapper.Map<OrganizationDisplayDto>), result.TotalItemCount);
    }

    public async Task<AppResponse> GetById(Guid id)
    {
        var org = await orgRepo.FindByIdAsync(id);
        return org == null
            ? new AppResponse { Success = false, Message = "Id not found" }
            : AppResponse.SuccessResponse(mapper.Map<OrganizationDisplayDto>(org));
    }

    private async Task<bool> TaxIdExist(string taxId)
    {
        return await orgRepo.ExistAsync(o => o.TaxId == taxId);
    }
}