using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Repositories;
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
                              IAppRepository<TaxOffice, int> taxRepo,
                              IMapper mapper) : IRegionAppService
{
    public async Task<AppResponse> CreateProvinceAsync(ProvinceCreateDto input)
    {
        var province = mapper.Map<Province>(input);
        var saved = await provinceRepo.CreateAsync(province);
        return AppResponse.SuccessResponse(saved);
    }

    public async Task<AppResponse> CreateManyProvincesAsync(List<ProvinceCreateDto> input)
    {
        try
        {
            var provinces = mapper.Map<List<Province>>(input);
            await provinceRepo.CreateManyAsync(provinces);
            return new AppResponse();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return AppResponse.ErrorResponse("Failed to create provinces");
        }
    }

    public async Task<AppResponse> CreateDistrictAsync(DistrictCreateDto input)
    {
        var province = await FindProvinceById(input.ProvinceId);
        if (province is null) return AppResponse.ErrorResponse("Province could not be found");
        var district = mapper.Map<District>(input);
        district.Province = province;
        var saved = await districtRepo.CreateAsync(district);
        return AppResponse.SuccessResponse(mapper.Map<DistrictDisplayDto>(saved));
    }

    public async Task<AppResponse> CreateManyDistrictsAsync(int provinceId, List<DistrictCreateDto> input)
    {
        try
        {
            var province = await FindProvinceById(provinceId);
            if (province is null) return AppResponse.ErrorResponse("Province could not be found");
            var districts = mapper.Map<List<District>>(input);
            foreach (var district in districts)
            {
                district.Province = province;
            }
            await districtRepo.CreateManyAsync(districts);
            return AppResponse.SuccessResponse("OK");
        }
        catch (Exception e)
        {
            logger.LogError("Error: {message}",e.Message);
            return AppResponse.ErrorResponse("Failed to create districts");
        }
    }

    public async Task<AppResponse> CreateTaxOfficeAsync(TaxOfficeCreateDto input)
    {
        var province = await FindProvinceById(input.ProvinceId);
        if (province is null) return AppResponse.ErrorResponse("Province could not be found");
        var taxOffice = mapper.Map<TaxOffice>(input);
        taxOffice.Province = province;
        var saved = await taxRepo.CreateAsync(taxOffice);
        return AppResponse.SuccessResponse(mapper.Map<TaxOfficeDisplayDto>(saved));
    }

    public async Task<AppResponse> CreateManyTaxOfficeAsync(int pId, List<TaxOfficeCreateDto> input)
    {
        try
        {
            var province = await FindProvinceById(pId);
            if (province is null) return AppResponse.ErrorResponse("Province could not be found");
            var taxOffices = mapper.Map<List<TaxOffice>>(input);
            foreach (var taxOffice in taxOffices)
            {
                taxOffice.Province = province;
            }

            await taxRepo.CreateManyAsync(taxOffices);
            return AppResponse.SuccessResponse("OK");
        }
        catch (Exception e)
        {
            logger.LogError("{message}",e.Message);
            return AppResponse.ErrorResponse("Failed to create tax offices");
        }
    }

    public async Task<AppResponse> GetAllProvincesAsync(PageRequest page)
    {
        var result = await provinceRepo.Find(x => !x.Deleted,
                                             sortBy: page.SortBy, order: page.OrderBy,
                                             include: [nameof(Province.Districts), nameof(Province.TaxOffices)])
                                       .AsSplitQuery()
                                       .ToPagedListAsync(page.Number, page.Size);

        return AppResponse.SuccessResponse(mapper.Map<IPagedList<ProvinceDisplayDto>>(result));
    }

    public async Task<AppResponse> GetProvinceAsync(int id)
    {
        var province = await FindProvinceById(id);
        return province == null
            ? AppResponse.ErrorResponse("Province could not be found")
            : AppResponse.SuccessResponse(mapper.Map<ProvinceDisplayDto>(province));
    }

    public async Task<AppResponse> GetDistrictsInProvinceAsync(int provinceId)
    {
        var districts = await districtRepo.Find(condition: d => d.Province.Id == provinceId,
                                                sortBy: "Id", order: "ASC")
                                          .ToListAsync();
        return AppResponse.SuccessResponse(districts.Select(mapper.Map<DistrictDisplayDto>));
    }

    public async Task<AppResponse> GetTaxOfficesInProvinceAsync(int provinceId)
    {
        var taxOffices = await taxRepo.Find(condition: t => t.Province.Id == provinceId,
                                            sortBy: "Id", order: "ASC")
                                      .AsNoTracking()
                                      .ToListAsync();
        return AppResponse.SuccessResponse(taxOffices.Select(mapper.Map<TaxOfficeDisplayDto>));
    }

    public async Task<AppResponse> GetTaxOfficesByParentAsync(int parentId)
    {
        var taxOffices = await taxRepo.Find(x => x.ParentId != null && x.ParentId == parentId, 
                                            sortBy: "Id", order: "ASC")
                                      .AsNoTracking()
                                      .ToListAsync();
        return AppResponse.SuccessResponse(taxOffices.Select(mapper.Map<TaxOfficeDisplayDto>));
    }

    private async Task<Province?> FindProvinceById(int provinceId)
    {
        return await provinceRepo.FindByIdAsync(provinceId);
    }
}