using AutoMapper;
using X.PagedList;

namespace WebApp.Services.Mappers;

public class PagedListConverter<TSource, TDestination> : ITypeConverter<IPagedList<TSource>, IPagedList<TDestination>>
{
    public IPagedList<TDestination> Convert(IPagedList<TSource> source, IPagedList<TDestination> destination, ResolutionContext context)
    {
        // Map the items
        var mappedItems = context.Mapper.Map<List<TDestination>>(source.ToList());

        // Create a new IPagedList<TDestination> with the mapped items and preserve pagination info
        return new StaticPagedList<TDestination>(mappedItems, source);
    }
}