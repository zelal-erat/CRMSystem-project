using System.Net;
using System.Text.Json;
using CRMSystem.Application.Common.Exceptions;
using FluentValidation;

namespace CRMSystem.Application.Common;

public class GlobalExceptionHandler : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
            CRMSystem.Application.Common.Exceptions.ValidationException validationEx => (HttpStatusCode.BadRequest, exception.Message, validationEx.Errors),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message, null),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message, null),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkisiz erişim", null),
            _ => (HttpStatusCode.InternalServerError, "Bir hata oluştu", null)
        };

        context.Response.StatusCode = (int)statusCode;

        var result = new
        {
            Success = false,
            Message = message,
            Errors = errors,
            StatusCode = statusCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
}
