using System.ComponentModel.DataAnnotations;

namespace IPMS.DTO.Dtos;

public class AuthSignupDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required.")]
    public required string FirstName {get; set;}

    public string? MiddleName {get; set;}

    public string? LastName {get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not a valid email address.")]
    public required string Email {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\+?[0-9]{7,15}$", ErrorMessage = "Phone number must be 7-15 digits, optionally starting with '+'.")]
    public required string PhoneNumber {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public required string Password {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password confirmation is required.")]
    public required string PasswordConfirm {get; set;}
}


public class AuthLoginDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not a valid email address.")]
    public required string Email {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required.")]
    public required string Password {get; set;}
}


// Admin-only: create an Admin, InsuranceAgent, or Underwriter account.
public class CreateStaffDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required.")]
    public required string FirstName {get; set;}

    public string? MiddleName {get; set;}

    public string? LastName {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not a valid email address.")]
    public required string Email {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\+?[0-9]{7,15}$", ErrorMessage = "Phone number must be 7-15 digits, optionally starting with '+'.")]
    public required string PhoneNumber {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public required string Password {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Role is required.")]
    public required string Role {get; set;}
}


// Admin-only: give an existing user another role (promote a customer to
// underwriter, agent, etc.).
public class AssignRoleDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Role is required.")]
    public required string Role {get; set;}
}
