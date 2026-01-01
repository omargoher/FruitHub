using System.Text.Json;
using FruitHub.API.DTOs;
using FruitHub.ApplicationCore.Exceptions;

namespace FruitHub.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        if (exception is AppException appException)
        {
            context.Response.StatusCode = appException.StatusCode;

            var response = exception switch
            {
                IdentityOperationException identityEx => new ErrorResponse
                {
                    Message = identityEx.Message,
                    Errors = identityEx.Errors
                },
                 
                _ => new ErrorResponse
                {
                    Message = appException.Message
                }
            };

            await context.Response.WriteAsJsonAsync(response);
            return;
        }
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new
            {
                message = "Internal server error"
            }));
    }
}
