using IPMS.DTO.Enum;

namespace IPMS.DTO.Entities;




public class Policy: BaseEntity
{
    public required string PolicyNumber {get; set;}
    public Guid? PreviousPolicyId {get; set;}
    public required Guid QuoteId { get; set; }
    public required Guid ProductId {get; set;}
    public required Guid CustomerId {get; set;}
    // Null when the policy was issued directly (no agent involved).
    public Guid? InsuranceAgentId {get; set;}
    public required Guid UnderWriterId {get; set;}
    public required decimal CoverageAmount {get; set;}
    public required decimal PremiumAmount {get; set;}
    public required DateOnly StartDate {get; set;}
    public required DateOnly EndDate {get; set;}
    public required DateOnly QuoteDate {get; set;}
    public required DateOnly IssueDate {get; set;}
    public required PolicyStatus Status {get; set;}
    public DateTimeOffset? DeletedAt {get; set;}
    public DateTimeOffset? CancellationDate {get; set;}
    public string? CancellationReason {get; set;}
}