using System.Collections;
using Microsoft.IdentityModel.Tokens;
using X.PagedList;

namespace WebApp.Payloads;

public class AppResponse
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public long? PageNumber { get; set; }
    public long? PageSize { get; set; }
    public long? PageCount { get; set; }
    public long? TotalCount { get; set; }
    public object? Data { get; set; }

    public static AppResponse Ok()
    {
        return new AppResponse{Success = true};
    }
    public static AppResponse Ok(string message)
    {
        return new AppResponse{Success = true, Message = message};
    }
    public static AppResponse SuccessResponse(object data)
    {
        AppResponse response = new()
        {
            Message = "OK",
            Data = data,
            TotalCount = 1
        };
        switch (data)
        {
            case IList list:
                response.TotalCount = list.Count;
                break;
            case IPagedList<object> pagedList:
                response.TotalCount = pagedList.TotalItemCount;
                response.PageNumber = pagedList.PageNumber;
                response.PageSize = pagedList.PageSize;
                response.PageCount = pagedList.PageCount;
                break;
        }
        return response;
    }

    public static AppResponse ErrorResponse(string mesage, params string[] details)
    {
        return new AppResponse
        {
            Success = false,
            Message = mesage,
            Data = !details.IsNullOrEmpty() ? details.ToList() : null
        };
    }
    
    public static AppResponse ErrorResponse(string mesage, List<string> details)
    {
        return new AppResponse
        {
            Success = false,
            Message = mesage,
            Data = details
        };
    }
}