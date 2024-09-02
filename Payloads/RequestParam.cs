
using WebApp.Enums;

namespace WebApp.Payloads
{
    public class RequestParam
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public string? SortBy { get; set; } = "Id";
        public string? OrderBy { get; set; } = SortOrder.ASC;
        public string? Keyword { get; set; }
    }
}
