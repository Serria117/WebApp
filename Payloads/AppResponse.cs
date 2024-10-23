using System.Collections;
using Microsoft.IdentityModel.Tokens;
using X.PagedList;

namespace WebApp.Payloads;

public class AppResponse
{
    public string? Code { get; set; }
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public long? PageNumber { get; set; }
    public long? PageSize { get; set; }
    public long? PageCount { get; set; }
    public long? TotalCount { get; set; }
    public object? Data { get; set; }

    public static AppResponse Ok()
    {
        return new AppResponse { Code = "200", Success = true };
    }

    public static AppResponse Ok(string message)
    {
        return new AppResponse { Code = "200", Success = true, Message = message };
    }

    public static AppResponse Ok(string code, string message)
    {
        return new AppResponse { Success = true, Message = message, Code = code };
    }
    public static AppResponse SuccessResponse(object data)
    {
        AppResponse response = new()
        {
            Code = "200",
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

    public static AppResponse Error(string mesage, params string[] details)
    {
        return new AppResponse
        {
            Success = false,
            Message = mesage,
            Data = !details.IsNullOrEmpty() ? details.ToList() : null
        };
    }

    public static AppResponse Error(string mesage, List<string> details)
    {
        return new AppResponse
        {
            Success = false,
            Message = mesage,
            Data = details
        };
    }

    public static AppResponse Error400(string message, params string[] details)
    {
        return new AppResponse
        {
            Code = "400",
            Success = false,
            Message = message,
            Data = details
        };
    }
    
    public static AppResponse Error404(string message, params string[] details)
    {
        return new AppResponse
        {
            Code = "404",
            Success = false,
            Message = message,
            Data = details
        };
    }

    public static AppResponse Error500(string message, params string[] details)
    {
        return new AppResponse
        {
            Code = "500",
            Success = false,
            Message = message,
            Data = details
        };
    }
}