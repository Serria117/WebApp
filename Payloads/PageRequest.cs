using WebApp.Enums;

namespace WebApp.Payloads;

public class PageRequest
{
    public int Skip { get; set; }
    public int Take { get; set; }
    public string SortBy { get; set; } = "Id";
    public string OrderBy { get; set; } = SortOrder.ASC;
    public int Number { get; set; }
    public int Size { get; set; }
    public string Sort { get; set; } = "Id ASC";
    public string? Keyword { get; set; }
    public int? Total { get; set; }

    public static PageRequest GetPaging(int pageNum, int pageSize, string? sortBy, string? sortOrder)
    {
        if (pageNum < 1) pageNum = 1;
        if (pageSize < 0) pageSize = 10;
        var page = new PageRequest
        {
            Number = pageNum,
            Size = pageSize,
            Skip = (pageNum - 1) * pageSize,
            Take = pageSize,
            SortBy = sortBy ?? "Id",
            OrderBy = sortOrder ?? SortOrder.ASC,
        };
        page.Sort = $"{page.SortBy} {page.OrderBy}";
        return page;
    }

    public static PageRequest GetPage(RequestParam req)
    {
        if (req.Page < 1) req.Page = 1;
        if (req.Size < 0) req.Size = 10;
        return new PageRequest
        {
            Number = req.Page,
            Size = req.Size,
            Skip = (req.Page - 1) * req.Size,
            Take = req.Size,
            Sort = $"{req.SortBy} {req.OrderBy}",
            Keyword = req.Keyword
        };
    }
}