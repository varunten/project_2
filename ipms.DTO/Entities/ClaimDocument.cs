namespace IPMS.DTO.Entities;


public class ClaimDocument: BaseEntity
{
    public required Guid ClaimId {get; set;}
    public required string FileName {get; set;}
    public required string FileType {get; set;}
    public required string FileURL {get; set;}
    public required DateTimeOffset UploadedAt {get; set;}
    public required Guid UploadedBy {get; set;}
}