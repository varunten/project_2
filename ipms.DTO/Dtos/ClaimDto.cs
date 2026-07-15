using System.ComponentModel.DataAnnotations;
using IPMS.DTO.Enum;

namespace IPMS.DTO.Dtos;


public class ClaimDto
{
    public required Guid Id { get; set; }
    public required string ClaimNumber { get; set; }
    public required Guid PolicyId { get; set; }
    public Guid? UnderWriterId { get; set; }
    public required DateOnly IncidentDate { get; set; }
    public DateOnly? SettledDate { get; set; }
    public required decimal ClaimAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public required ClaimStatus Status { get; set; }
    public required DateTimeOffset CreatedAt {get;set;}
    public required DateTimeOffset UpdatedAt {get;set;}
}



public class ClaimsDto
{
    public required ulong Total { get; set; }
    public required List<ClaimDto> Claims { get; set; }
}



public class CreateClaimDto
{
    public required Guid PolicyId { get; set; }
    public required DateOnly IncidentDate { get; set; }

    [Range(1, 100000000, ErrorMessage = "Claim amount must be greater than zero.")]
    public required decimal ClaimAmount { get; set; }

    public string? Reason { get; set; }
}



public class UpdateClaimDto
{
    public Guid? UnderWriterId { get; set; }
    public ClaimStatus? Status { get; set; }
    public string? Notes { get; set; }
    public decimal? ClaimAmount { get; set; }
}



public class ClaimDocumentDto
{
    public required Guid Id { get; set; }
    public required Guid ClaimId { get; set; }
    public required string FileName { get; set; }
    public required string FileType { get; set; }
    public required string FileURL { get; set; }
    public required DateTimeOffset UploadedAt { get; set; }
    public required Guid UploadedBy { get; set; }
}



public class UploadClaimDocumentDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "File name is required.")]
    public required string FileName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "File type is required.")]
    public required string FileType { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "File URL is required.")]
    [Url(ErrorMessage = "File URL must be a valid URL.")]
    public required string FileURL { get; set; }
    // UploadedBy is taken from the signed-in user, not the request body.
}