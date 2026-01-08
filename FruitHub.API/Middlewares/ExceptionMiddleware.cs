using FruitHub.API.DTOs;
using FruitHub.ApplicationCore.Exceptions;

namespace FruitHub.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        ErrorResponse response;
        int statusCode;

        if (exception is AppException appException)
        {
            statusCode = appException.StatusCode;

            response = exception switch
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
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;

            response = new ErrorResponse
            {
                Message = "Internal server error"
            };
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}