using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
            logger.LogWarning("Something went wrong: {message} - TracedId: {tracedId} - At time: {time}", ex.Message, tracedId, DateTime.Now.ToString(CultureInfo.CurrentCulture));
            logger.LogError("Stack trace: {st}", ex.StackTrace);
            await HandleExceptionAsync(httpContext, ex, tracedId);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, string tracedId)
    {
        context.Response.ContentType = ContentType.ApplicationJson;
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var message =
            "Something went wrong while the server's processing your request. Please use the traceId to report the error.";
        if (exception is ValidationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            message = exception.Message;
        }
        var response = new
        {
            status = context.Response.StatusCode,
            tracedId = tracedId,
            message = message,
            //detailed = exception.Message // This can be omitted in production to avoid exposing sensitive information
        };

        return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}