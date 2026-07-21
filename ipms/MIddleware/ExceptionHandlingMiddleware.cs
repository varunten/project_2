using System.Security.Claims;
using IPMS.DAL.Data;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;
using IPMS.DTO.Exceptions;

namespace IPMS.Middlewares;


// Catches everything thrown further down the pipeline, records it in the
// ErrorLogs table, and turns it into a consistent ErrorResponse body with the
// right HTTP status code.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
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
            await SaveErrorLogAsync(context, ex, ex.StatusCode);
            await WriteError(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            // Anything unexpected - log it and hide the details from the client.
            _logger.LogError(ex, "Unhandled exception");
            await SaveErrorLogAsync(context, ex, 500);
            await WriteError(context, 500, "An unexpected error occurred.");
        }
    }


    private async Task SaveErrorLogAsync(
        HttpContext context,
        Exception exception,
        int statusCode)
    {
        try
        {
            // Use a brand-new DbContext from its own scope. The request's own
            // context may still be holding the failed changes (for example when
            // the exception came out of SaveChanges), and reusing it would just
            // fail again.
            using IServiceScope scope = _scopeFactory.CreateScope();
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            string? userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = Guid.TryParse(userIdValue, out Guid parsed) ? parsed : null;

            db.ErrorLogs.Add(new ErrorLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Message = Truncate(BuildMessage(exception), 2000) ?? "(no message)",
                ExceptionType = exception.GetType().Name,
                // ToString() (not StackTrace) so the inner exception chain -
                // where database errors describe themselves - is kept too.
                StackTrace = Truncate(exception.ToString(), 4000),
                Path = Truncate(context.Request.Path.Value, 512),
                Method = context.Request.Method,
                StatusCode = statusCode,
                Timestamp = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }
        catch (Exception loggingException)
        {
            // Writing the log must never break the response the user gets
            // (e.g. if the database itself is what failed).
            _logger.LogError(loggingException, "Could not write to ErrorLogs");
        }
    }


    // Wrappers like DbUpdateException say only "see the inner exception", so
    // pull the real cause out - that is where SQL errors describe themselves.
    private static string BuildMessage(Exception exception)
    {
        Exception cause = exception;
        while (cause.InnerException is not null)
        {
            cause = cause.InnerException;
        }

        return ReferenceEquals(cause, exception)
            ? exception.Message
            : $"{exception.Message} -> {cause.Message}";
    }


    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..maxLength];
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
