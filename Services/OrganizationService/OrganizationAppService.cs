using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.OrganizationService.Dto;
using WebApp.Services.UserService;
using X.Extensions.PagedList.EF;
using X.PagedList;

namespace WebApp.Services.OrganizationService;

public interface IOrganizationAppService
{
    Task<AppResponse> Create(OrganizationInputDto dto);

    Task<AppResponse> CreateMany(List<OrganizationInputDto> dto);
    Task<AppResponse> Find(PageRequest page);
    Task<AppResponse> GetOneById(Guid id);
    Task<AppResponse> CheckTaxIdExist(string taxId);
    Task<AppResponse> Update(Guid orgId, OrganizationInputDto updateDto);
}

public class OrganizationAppService(IAppRepository<Organization, Guid> orgRepo,
                                    IAppRepository<Province, int> provinceRepo,
                                    IAppRepository<District, int> districtRepo,
                                    IAppRepository<TaxOffice, int> taxOfficeRepo,
                                    IUserManager userManager) : AppServiceBase(userManager), IOrganizationAppService
{
    public async Task<AppResponse> Create(OrganizationInputDto dto)
    {
        if (await TaxIdExist(dto.TaxId))
            return AppResponse.ErrorResponse("Tax Id has already existed.");

        var invalidMessage = await ValidInputDto(dto);
        if (!invalidMessage.IsNullOrEmpty()) return AppResponse.ErrorResponse("Invalid input", invalidMessage);

        var newOrg = dto.ToEntity();
        newOrg.District = districtRepo.Attach(dto.DistrictId!.Value);
        newOrg.TaxOffice = taxOfficeRepo.Attach(dto.TaxOfficeId!.Value);

        var saved = await orgRepo.CreateAsync(newOrg);
        return AppResponse.SuccessResponse(saved.ToDisplayDto());
    }

    public async Task<AppResponse> CreateMany(List<OrganizationInputDto> input)
    {
        var duplicateTaxIds = input.GroupBy(o => o.TaxId)
                                   .Where(c => c.Count() > 1)
                                   .SelectMany(o => o).ToList();

        var distinctTaxIds = input.GroupBy(o => o.TaxId)
                                  .Where(o => o.Count() == 1)
                                  .SelectMany(o => o).ToList();

        var currentTaxIds = orgRepo.GetQueryable().Select(d => d.TaxId).ToHashSet();

        var existingTaxIds = distinctTaxIds.Where(x => currentTaxIds.Contains(x.TaxId)).ToList();

        var validTaxIds = distinctTaxIds.Except(existingTaxIds).ToList(); //the dto list contains only passed taxId

        var taxOfficeIds = taxOfficeRepo.GetQueryable().Select(t => t.Id).ToHashSet();
        var districtIds = districtRepo.GetQueryable().Select(d => d.Id).ToHashSet();

        var invalidTaxOfficeIds = input.Where(x => x.TaxOfficeId is null || !taxOfficeIds.Contains(x.TaxOfficeId.Value))
                                       .ToList();
        var invalidDistrictIds = input.Where(x => x.DistrictId is null || !districtIds.Contains(x.DistrictId.Value))
                                      .ToList();

        var validDtos = validTaxIds.Except(invalidTaxOfficeIds).Except(invalidDistrictIds).ToList();

        if (validDtos.IsNullOrEmpty())
            return new AppResponse
            {
                Message = "All inputs are invalid",
                Success = false,
                Data = new
                {
                    totalItems = input.Count,
                    insertedItems = 0,
                    invalidItems = new
                    {
                        duplicateTaxIds, existingTaxIds, invalidTaxOfficeIds, invalidDistrictIds
                    }
                }
            };

        var entitiesToSave = validDtos.Select(dto =>
        {
            var org = dto.ToEntity();
            org.TaxOffice = taxOfficeRepo.Attach(dto.TaxOfficeId!.Value);
            org.District = districtRepo.Attach(dto.DistrictId!.Value);
            return org;
        }).ToList();

        await orgRepo.CreateManyAsync(entitiesToSave);

        return new AppResponse
        {
            Success = true,
            Message =
                $"Successfully added {entitiesToSave.Count}/{input.Count} organization(s). Check data for error (if any)",
            Data = new
            {
                totalItems = input.Count,
                insertedItems = entitiesToSave.Count,
                invalidItems = new
                {
                    duplicateTaxIds, existingTaxIds, invalidTaxOfficeIds, invalidDistrictIds
                }
            }
        };
    }

    public async Task<AppResponse> CheckTaxIdExist(string taxId)
    {
        return await TaxIdExist(taxId)
            ? new AppResponse { Success = false, Message = "TaxId has already existed" }
            : new AppResponse { Success = true, Message = "OK" };
    }

    public async Task<AppResponse> Find(PageRequest page)
    {
        var keyword = page.Keyword.RemoveSpace()?.UnSign();
        var province = provinceRepo.GetQueryable();
        var pagedResult = (await orgRepo.Find(condition: o => !o.Deleted && (string.IsNullOrEmpty(keyword) ||
                                                                             o.UnsignName.Contains(keyword) ||
                                                                             o.ShortName == null ||
                                                                             o.ShortName.Contains(keyword) ||
                                                                             o.TaxId.Contains(keyword)),
                                              sortBy: page.SortBy, order: page.OrderBy,
                                              include:
                                              [
                                                  nameof(Organization.TaxOffice),
                                                  nameof(Organization.District)
                                              ])
                                        .AsSplitQuery()
                                        .AsNoTracking()
                                        .ToPagedListAsync(page.Number, page.Size))
            .MapPagedList(x => x.ToDisplayDto());
        return AppResponse.SuccessResponse(pagedResult);
    }

    public async Task<AppResponse> Update(Guid orgId, OrganizationInputDto updateDto)
    {
        var invalidMessage = await ValidInputDto(updateDto);
        if (!invalidMessage.IsNullOrEmpty()) return AppResponse.ErrorResponse("Invalid input", invalidMessage);

        var foundOrg = await orgRepo.Find(o => o.Id == orgId && !o.Deleted).FirstOrDefaultAsync();
        if (foundOrg is null)
            return new AppResponse { Success = false, Message = "Organization Id not found" };
        if (await TaxIdExist(updateDto.TaxId) && updateDto.TaxId != foundOrg.TaxId)
        {
            return new AppResponse { Success = false, Message = "The TaxId you enter has already existed" };
        }

        updateDto.UpdateEntity(foundOrg);

        foundOrg.District = districtRepo.Attach(updateDto.DistrictId!.Value);
        foundOrg.TaxOffice = taxOfficeRepo.Attach(updateDto.TaxOfficeId!.Value);
        var saved = await orgRepo.UpdateAsync(foundOrg);
        return new AppResponse { Success = true, Data = saved.Id, Message = "Update successfully" };
    }

    public async Task<AppResponse> GetOneById(Guid id)
    {
        var org = await orgRepo.Find(condition: x => x.Id == id,
                                     include:
                                     [
                                         nameof(Organization.TaxOffice),
                                         nameof(Organization.District)
                                     ])
                               .FirstOrDefaultAsync();
        return org == null
            ? AppResponse.ErrorResponse("Id not found")
            : AppResponse.SuccessResponse(org.ToDisplayDto());
    }

    private async Task<bool> TaxIdExist(string taxId)
    {
        return await orgRepo.ExistAsync(o => o.TaxId == taxId);
    }

    private async Task<List<string>> ValidInputDto(OrganizationInputDto dto)
    {
        var errors = new List<string>();

        if (dto.TaxOfficeId is null || !await taxOfficeRepo.ExistAsync(x => x.Id == dto.TaxOfficeId))
        {
            errors.Add("Invalid tax office");
        }

        if (dto.DistrictId is null || !await districtRepo.ExistAsync(x => x.Id == dto.DistrictId))
        {
            errors.Add("Invalid district");
        }
        return errors;
    }
}