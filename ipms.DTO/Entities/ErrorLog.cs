namespace IPMS.DTO.Entities;


// One row per exception that reaches the global exception handler: business
// errors (404/409/...), unexpected failures (500) and database errors.
// Immutable once written, so it does NOT inherit BaseEntity.
public class ErrorLog
{
    public Guid Id { get; set; }

    // Who was making the request (null when nobody was signed in).
    public Guid? UserId { get; set; }

    public required string Message { get; set; }

    // e.g. "NotFoundException", "SqlException".
    public string? ExceptionType { get; set; }

    public string? StackTrace { get; set; }

    // The request that failed, e.g. "POST /api/quote".
    public string? Path { get; set; }
    public string? Method { get; set; }

    // HTTP status we returned to the caller (500 = unexpected).
    public int StatusCode { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}
