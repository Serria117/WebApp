using Microsoft.OpenApi.Extensions;

namespace WebApp.Payloads;

public class ErrorResponse
{
    public Error Code { get; set; }
    public string? Message { get; set; }

    public static ErrorResponse InvalidParams()
    {
        return new ErrorResponse
        {
            Code = Error.InvalidParams,
            Message = Error.InvalidParams.GetDisplayName()
        };
    }
}