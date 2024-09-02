﻿using X.PagedList;

namespace WebApp.Payloads;

public class AppResponse
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public long? PageNumber { get; set; }
    public long? PageSize { get; set; }
    public long? TotalCount { get; set; }
    public object? Data { get; set; }

    public static AppResponse SuccessResponse(object data)
    {
        return new AppResponse
        {
            Data = data,
            Message = "OK"
        };
    }
    
    public static AppResponse SuccessResponse(object data, PageRequest page)
    {
        return new AppResponse
        {
            Data = data,
            Message = "OK",
            PageNumber = page.Number,
            PageSize = page.Size,
            TotalCount = page.Total
        };
    }

    public static AppResponse SuccessResponsePaged<T>(IPagedList<T> data, int totalCount)
    {
        return new AppResponse
        {
            Data = data.ToList(),
            Message = "OK",
            PageNumber = data.PageNumber,
            PageSize = data.PageSize,
            TotalCount = totalCount
        };
    }
}

public class AppResponse<T> : AppResponse
{
    public List<T> Items { get; set; } = [];
    
    public static AppResponse<T> SuccessResponse(List<T> items, long count)
    {
        return new AppResponse<T>
        {
            Items = items,
            Message = "OK"
        };
    }
}