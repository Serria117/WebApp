using WebApp.Enums;

namespace WebApp.Services.CommonService;

public class QueryService
{
    public static string BuildSortQuery(string property, string order = SortOrder.ASC)
    {
        return $"{property} {order}";
    }
}