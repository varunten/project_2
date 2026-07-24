
using System.ComponentModel.DataAnnotations;
using IPMS.DTO.Enum;

namespace IPMS.DTO.Dtos;



public class PolicyDto
{
    public required Guid Id { get; set; }
    public required string PolicyNumber { get; set; }
    // Set when this policy is a renewal of an earlier one.
    public Guid? PreviousPolicyId { get; set; }
    public required Guid QuoteId { get; set; }
    public required Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public Guid? InsuranceAgentId {get;set;}
    public required Guid UnderWriterId {get;set;}
    public required Guid CustomerId { get; set; }
    // Friendly, non-identifying labels for display. Populated when a single
    // policy is fetched for its details view; left null in list responses.
    public string? UnderwriterName { get; set; }
    public string? UnderwriterEmail { get; set; }
    public string? InsuranceAgentName { get; set; }
    public string? InsuranceAgentEmail { get; set; }
    public string? QuoteNumber { get; set; }
    public string? PreviousPolicyNumber { get; set; }
    public required decimal CoverageAmount { get; set; }
    public required decimal PremiumAmount { get; set; }
    public required DateOnly QuoteDate { get; set; }
    public required DateOnly IssueDate { get; set; }
    public required DateOnly StartDate { get; set; }
    public required DateOnly EndDate { get; set; }
    public required PolicyStatus Status { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }
}

public class PoliciesDto
{
    public required ulong Total { get; set; }
    public required List<PolicyDto> Policies { get; set; }
}


public class CancelPolicyDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Cancellation reason is required.")]
    public required string CancellationReason { get; set; }
}