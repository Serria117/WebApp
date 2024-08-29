namespace WebApp.Payloads;

public class PageRequest
{
    public int Skip { get; set; }
    public int Take { get; set; }
    public string SortBy { get; set; } = "Id";
    public string SortOrder { get; set; } = "ASC";
    public int Page { get; set; }
    public int Size { get; set; }

    public static PageRequest GetPaging(int pageNum, int pageSize, string? sortBy, string? sortOrder)
    {
        if (pageNum < 1) pageNum = 1;
        if (pageSize < 0) pageSize = 10;
        return new PageRequest
        {
            Page = pageNum,
            Size = pageSize,
            Skip = (pageNum - 1) * pageSize,
            Take = pageSize,
            SortBy = sortBy ?? "Id",
            SortOrder = sortOrder ?? "ASC"
        };
    }
}