
namespace IPMS.DTO.Dtos;

public class UserDto
{
    public required string FirstName {get; set;}
    public string? MiddleName {get; set;}
    public string? LastName {get; set;}
    public required string Email {get; set;}
    public required string PhoneNumber {get; set;}
    public required DateTimeOffset CreatedAt {get; set;}
    public required DateTimeOffset UpdatedAt {get; set;}
}   

public class UpdateUserDto {
    public string? FirstName {get; set;}
    public string? MiddleName {get; set;}
    public string? LastName {get; set;}
    public string? Email {get; set;}
    public string? PhoneNumber {get; set;}
}