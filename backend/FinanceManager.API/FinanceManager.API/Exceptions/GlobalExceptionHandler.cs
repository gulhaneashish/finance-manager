using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "An unhandled exception occurred.");

        var statusCode = exception switch
        {
            InvalidOperationException =>
                StatusCodes.Status400BadRequest,

            KeyNotFoundException =>
                StatusCodes.Status404NotFound,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        var response = new ProblemDetails
        {
            Status = statusCode,

            Title = statusCode == 500
                ? "An unexpected error occurred."
                : "Request failed.",

            Detail = statusCode == 500
                ? "Something went wrong on the server."
                : exception.Message,

            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode =
            statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}