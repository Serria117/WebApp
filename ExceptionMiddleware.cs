using System.Net;

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
            logger.LogError($"Something went wrong: {ex.Message} - TracedId: {tracedId}");
            await HandleExceptionAsync(httpContext, ex, tracedId);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, string tracedId)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new
        {
            status =context.Response.StatusCode,
            tracedId = tracedId,
            message = "Something wrong with your request.",
            detailed = exception.Message // This can be omitted in production to avoid exposing sensitive information
        };

        return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}