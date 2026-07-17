namespace IPMS.DTO.Entities;


// One row per insert / update / delete that happens through the DbContext.
// Immutable once written, so it does NOT inherit BaseEntity (no UpdatedAt).
public class AuditLog
{
    public Guid Id { get; set; }

    // Who made the change (null = system action, e.g. seeding, or an anonymous
    // request like a failed login).
    public Guid? UserId { get; set; }

    // "Added", "Modified" or "Deleted".
    public required string Action { get; set; }

    // The table/entity that changed, e.g. "Policies".
    public required string TableName { get; set; }

    // Primary key of the affected row.
    public required string RecordId { get; set; }

    // For updates: the comma-separated list of columns that changed.
    public string? ChangedColumns { get; set; }

    // When it happened (UTC).
    public DateTimeOffset Timestamp { get; set; }
}
