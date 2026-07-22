using System.ComponentModel.DataAnnotations;

namespace IPMS.DTO.Dtos;

public class AuthSignupDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
    public required string FirstName {get; set;}

    public string? MiddleName {get; set;}

    public string? LastName {get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required.")]
    [RegularExpression(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)?\.[a-zA-Z]{2,}$",
        ErrorMessage = "Enter a valid email address (e.g. name@example.com).")]
    public required string Email {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits (US).")]
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
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
    public required string FirstName {get; set;}

    public string? MiddleName {get; set;}

    public string? LastName {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required.")]
    [RegularExpression(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)?\.[a-zA-Z]{2,}$",
        ErrorMessage = "Enter a valid email address (e.g. name@example.com).")]
    public required string Email {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits (US).")]
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
