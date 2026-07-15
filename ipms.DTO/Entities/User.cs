namespace IPMS.DTO.Entities;


public class User: BaseEntity
{
    public required string FirstName {get; set;}
    public string? MiddleName {get; set;}
    public string? LastName {get; set;}
    public required string Email {get; set;}
    public required string PasswordHash {get; set;}
    public required string PhoneNumber {get; set;}
    public DateTimeOffset? DeletedAt {get; set;}
}


