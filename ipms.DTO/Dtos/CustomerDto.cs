
using System.ComponentModel.DataAnnotations;
using IPMS.DTO.Enum;

namespace IPMS.DTO.Dtos;


public class CustomerAddressDto
{
    public ulong? HouseNumber {get; set;}
    public ulong? StreetNumber {get; set;}
    public string? StreetName {get; set;}
    public string? StreetSuffix {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "City is required.")]
    public required string City {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "State is required.")]
    public required string State {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "Zip code is required.")]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Zip code must be 5 digits (optionally ZIP+4).")]
    public required string ZipCode {get; set;}
}


public class UpdateCustomerAddressDto
{
    public ulong? HouseNumber {get; set;}
    public ulong? StreetNumber {get; set;}
    public string? StreetName {get; set;}
    public string? StreetSuffix {get; set;}
    public string? City {get; set;}
    public string? State {get; set;}
    public string? ZipCode {get; set;}
}


public class CustomerDto
{
    public required Guid Id {get; set;}
    public required string FirstName {get; set;}
    public string? MiddleName {get; set;}
    public string? LastName {get; set;}
    public required string Email {get; set;}
    public required string PhoneNumber {get; set;}
    public required DateOnly DateOfBirth {get; set;}
    public required CustomerGender Gender {get; set;}
    public required MaritalStatus MaritalStatus {get;set;}
    public required CustomerAddressDto Address {get; set;}
    public required DateTimeOffset CreatedAt {get; set;}
    public required DateTimeOffset UpdatedAt {get; set;}
}


public class CreateCustomerDto
{
    public required DateOnly DateOfBirth {get; set;}
    public required CustomerGender Gender {get; set;}
    public required MaritalStatus MaritalStatus {get; set;}

    [Required(ErrorMessage = "Address is required.")]
    public required CustomerAddressDto Address {get; set;}

    [Required(AllowEmptyStrings = false, ErrorMessage = "SSN is required.")]
    [RegularExpression(@"^\d{3}-?\d{2}-?\d{4}$", ErrorMessage = "SSN must be 9 digits (e.g. 123-45-6789).")]
    public required string SSN {get; set;}
}


public class CustomersDto
{
    public required ulong Total {get; set;}
    public required List<CustomerDto> Customers {get; set;}
}


public class UpdateCustomerDto
{
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
    public string? FirstName {get; set;}

    public string? MiddleName {get; set;}
    public string? LastName {get; set;}

    // Optional on update, but must still be valid when supplied.
    [RegularExpression(
        @"^[a-zA-Z0-9._%+-]+@(?!.*\.(?:com|net|org|edu|gov|mil|info|biz|io)\.)[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}$",
        ErrorMessage = "Enter a valid email address (e.g. name@example.com).")]
    public string? Email {get; set;}

    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits (US).")]
    public string? PhoneNumber {get; set;}
    public DateOnly? DateOfBirth {get; set;}
    public CustomerGender? Gender {get; set;}
    public MaritalStatus? MaritalStatus {get; set;}
    public UpdateCustomerAddressDto? Address {get; set;}
}
