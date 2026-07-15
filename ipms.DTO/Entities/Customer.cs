using IPMS.DTO.Enum;

namespace IPMS.DTO.Entities;




public class Customer: BaseEntity
{
    public required Guid UserId {get; set;}
    public required DateOnly DateOfBirth {get; set;}
    public required CustomerGender Gender {get; set;}
    public required MaritalStatus MaritalStatus {get; set;}
    public required string SSNHash {get; set;}
    public DateTimeOffset? DeletedAt {get; set;}
}