using AutoMapper;
using X.PagedList;

namespace WebApp.Services.Mappers;

public class PagedMapper : Profile
{
    public PagedMapper()
    {
        CreateMap(typeof(IPagedList<>), typeof(IPagedList<>)).ConvertUsing(typeof(PagedListConverter<,>));
    }
}