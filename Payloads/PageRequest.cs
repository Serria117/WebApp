using WebApp.Enums;

namespace WebApp.Payloads;

public class PageRequest
{
    public string SortBy { get; set; } = "Id";
    public string OrderBy { get; set; } = SortOrder.ASC;
    public int Number { get; set; }
    public int Size { get; set; }
    public string Sort { get; set; } = "Id ASC";
    public string? Keyword { get; set; }
    public int? Total { get; set; }

    public static PageRequest GetPage(RequestParam req)
    {
        if (req.Page < 1) req.Page = 1;
        if (req.Size < 0) req.Size = 10;
        return new PageRequest
        {
            Number = req.Page ?? 1,
            Size = req.Size ?? 10,
            SortBy = req.SortBy ?? "Id",
            OrderBy = req.OrderBy ?? SortOrder.ASC,
            Sort = $"{req.SortBy} {req.OrderBy}",
            Keyword = req.Keyword
        };
    }
}