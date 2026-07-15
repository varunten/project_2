using IPMS.DTO.Dtos;
using IPMS.DTO.Exceptions;

namespace IPMS.Middlewares;


// Catches everything thrown further down the pipeline and turns it into a
// consistent ErrorResponse body with the right HTTP status code.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (AppException ex)
        {
            // An error we threw on purpose - use its status code and message.
            await WriteError(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            // Anything unexpected - log it and hide the details from the client.
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, 500, "An unexpected error occurred.");
        }
    }

    private static async Task WriteError(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Success = false,
            Message = message
        });
    }
}
