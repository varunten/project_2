using IPMS.DTO.Enum;

namespace IPMS.DTO.Entities;




public class Claim: BaseEntity
{
    public required string ClaimNumber {get; set;}
    public required Guid PolicyId {get; set;}
    // Assigned when an underwriter picks the claim up for review; null until then.
    public Guid? UnderWriterId {get; set;}
    public required DateOnly IncidentDate {get; set;}
    public DateOnly? SettledDate {get; set;}
    public required decimal ClaimAmount {get; set;}
    public decimal? ApprovedAmount {get; set;}
    public string? Reason {get; set;}
    public string? Notes {get; set;}
    public required ClaimStatus Status {get; set;}
}