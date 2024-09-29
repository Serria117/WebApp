using System.Net;
using WebApp.Enums;

namespace WebApp;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            var tracedId = Ulid.NewUlid().ToString();
            logger.LogWarning("Something went wrong: {message} - TracedId: {tracedId}", ex.Message, tracedId);
            logger.LogError("Stack trace: {st}", ex.StackTrace);
            await HandleExceptionAsync(httpContext, ex, tracedId);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, string tracedId)
    {
        context.Response.ContentType = ContentType.ApplicationJson;
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            status = context.Response.StatusCode,
            tracedId = tracedId,
            message = "Something went wrong while the server's processing your request. Please use the traceId to report the error.",
            detailed = exception.Message // This can be omitted in production to avoid exposing sensitive information
        };

        return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}