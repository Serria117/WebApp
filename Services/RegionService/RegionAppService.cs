using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.Mappers;
using WebApp.Services.RegionService.Dto;
using X.Extensions.PagedList.EF;
using X.PagedList;

namespace WebApp.Services.RegionService;

public interface IRegionAppService
{
    Task<AppResponse> CreateProvinceAsync(ProvinceCreateDto input);
    Task<AppResponse> CreateDistrictAsync(DistrictCreateDto input);
    Task<AppResponse> CreateTaxOfficeAsync(TaxOfficeCreateDto input);
    Task<AppResponse> GetAllProvincesAsync(PageRequest page);
    Task<AppResponse> GetProvinceAsync(int id);
    Task<AppResponse> GetDistrictsInProvinceAsync(int provinceId);
    Task<AppResponse> GetTaxOfficesInProvinceAsync(int provinceId);
    Task<AppResponse> CreateManyProvincesAsync(List<ProvinceCreateDto> input);
    Task<AppResponse> CreateManyDistrictsAsync(int provinceId, List<DistrictCreateDto> input);
    Task<AppResponse> GetTaxOfficesByParentAsync(int parentId);
    Task<AppResponse> CreateManyTaxOfficeAsync(int pId, List<TaxOfficeCreateDto> input);
}

public class RegionAppService(ILogger<RegionAppService> logger,
                              IAppRepository<Province, int> provinceRepo,
                              IAppRepository<District, int> districtRepo,
                              IAppRepository<TaxOffice, int> taxRepo) : IRegionAppService
{
    public async Task<AppResponse> CreateProvinceAsync(ProvinceCreateDto input)
    {
        var province = input.ToEntity();
        var saved = await provinceRepo.CreateAsync(province);
        return AppResponse.SuccessResponse(saved.ToDisplayDto());
    }

    public async Task<AppResponse> CreateManyProvincesAsync(List<ProvinceCreateDto> input)
    {
        try
        {
            var provinces = input.MapCollection(x => x.ToEntity()).ToList();
            await provinceRepo.CreateManyAsync(provinces);
            return new AppResponse();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return AppResponse.Error("Failed to create provinces");
        }
    }

    public async Task<AppResponse> CreateDistrictAsync(DistrictCreateDto input)
    {
        if (!await provinceRepo.ExistAsync(x => x.Id == input.ProvinceId))
            return AppResponse.Error("Province could not be found");
        var district = input.ToEntity(provinceRepo);
        var saved = await districtRepo.CreateAsync(district);
        return AppResponse.SuccessResponse(saved.ToDisplayDto());
    }

    public async Task<AppResponse> CreateManyDistrictsAsync(int provinceId, List<DistrictCreateDto> input)
    {
        try
        {
            if (!await provinceRepo.ExistAsync(x => x.Id == provinceId))
                return AppResponse.Error("Province could not be found");
            var districts = input.MapCollection(x => x.ToEntity(provinceRepo)).ToList();
            await districtRepo.CreateManyAsync(districts);
            return AppResponse.SuccessResponse("OK");
        }
        catch (Exception e)
        {
            logger.LogError("Error: {message}", e.Message);
            return AppResponse.Error("Failed to create districts");
        }
    }

    public async Task<AppResponse> CreateTaxOfficeAsync(TaxOfficeCreateDto input)
    {
        if (!await provinceRepo.ExistAsync(x => x.Id == input.ProvinceId))
            return AppResponse.Error("Province could not be found");
        var taxOffice = input.ToEntity(provinceRepo);
        var saved = await taxRepo.CreateAsync(taxOffice);
        return AppResponse.SuccessResponse(saved.ToDisplayDto());
    }

    public async Task<AppResponse> CreateManyTaxOfficeAsync(int pId, List<TaxOfficeCreateDto> input)
    {
        try
        {
            if (!await provinceRepo.ExistAsync(x => x.Id == pId))
                return AppResponse.Error("Province could not be found");
            var taxOffices = input.MapCollection(x => x.ToEntity(provinceRepo)).ToList();

            await taxRepo.CreateManyAsync(taxOffices);
            return AppResponse.SuccessResponse("OK");
        }
        catch (Exception e)
        {
            logger.LogError("{message}", e.Message);
            return AppResponse.Error("Failed to create tax offices");
        }
    }

    public async Task<AppResponse> GetAllProvincesAsync(PageRequest page)
    {
        var result = await provinceRepo.Find(x => !x.Deleted,
                                             sortBy: page.SortBy, order: page.OrderBy,
                                             include: [nameof(Province.Districts), nameof(Province.TaxOffices)])
                                       .AsSplitQuery()
                                       .ToPagedListAsync(page.Number, page.Size);

        return AppResponse.SuccessResponse(result.MapPagedList(x => x.ToDisplayDto()));
    }

    public async Task<AppResponse> GetProvinceAsync(int id)
    {
        var province = await provinceRepo.FindByIdAsync(id);
        return province == null
            ? AppResponse.Error("Province could not be found")
            : AppResponse.SuccessResponse(province.ToDisplayDto());
    }

    public async Task<AppResponse> GetDistrictsInProvinceAsync(int provinceId)
    {
        var districts = await districtRepo.Find(condition: d => d.Province.Id == provinceId,
                                                sortBy: "Id", order: "ASC")
                                          .ToListAsync();
        return AppResponse.SuccessResponse(districts.MapCollection(x => x.ToDisplayDto()));
    }

    public async Task<AppResponse> GetTaxOfficesInProvinceAsync(int provinceId)
    {
        var taxOffices = await taxRepo.Find(condition: t => t.Province.Id == provinceId,
                                            sortBy: "Id", order: "ASC")
                                      .AsNoTracking()
                                      .ToListAsync();
        return AppResponse.SuccessResponse(taxOffices.MapCollection(x => x.ToDisplayDto()));
    }

    public async Task<AppResponse> GetTaxOfficesByParentAsync(int parentId)
    {
        var taxOffices = await taxRepo.Find(x => x.ParentId != null && x.ParentId == parentId,
                                            sortBy: "Id", order: "ASC")
                                      .AsNoTracking()
                                      .ToListAsync();
        return AppResponse.SuccessResponse(taxOffices.MapCollection(x => x.ToDisplayDto()));
    }

}