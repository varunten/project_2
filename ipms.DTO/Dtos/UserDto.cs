
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

// Admin's view of a user: includes the Id (needed to assign roles) and the
// roles they currently hold.
public class UserWithRolesDto
{
    public required Guid Id {get; set;}
    public required string FirstName {get; set;}
    public string? MiddleName {get; set;}
    public string? LastName {get; set;}
    public required string Email {get; set;}
    public required string PhoneNumber {get; set;}
    public List<string> Roles {get; set;} = [];
    public required DateTimeOffset CreatedAt {get; set;}
}


public class UsersDto
{
    public required ulong Total {get; set;}
    public required List<UserWithRolesDto> Users {get; set;}
}


public class UpdateUserDto {
    public string? FirstName {get; set;}
    public string? MiddleName {get; set;}
    public string? LastName {get; set;}
    public string? Email {get; set;}
    public string? PhoneNumber {get; set;}
}