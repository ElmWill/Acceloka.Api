namespace Acceloka.Api.Common;

using System.Net;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        int statusCode = ex switch
        {
            ApiExceptions apiEx => apiEx.StatusCode,
            _ => (int)HttpStatusCode.InternalServerError
        };

        string message = ex.Message;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title = "Request Error",
            status = statusCode,
            detail = message,
            instance = context.Request.Path
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

}
