namespace IPMS.DTO.Entities;



public class CustomerAddress: BaseEntity
{
    public required Guid CustomerId {get; set;}
    public ulong? HouseNumber {get; set;}
    public ulong? StreetNumber {get; set;}
    public string? StreetName {get; set;}
    public string? StreetSuffix {get; set;}
    public required string City {get; set;}
    public required string State {get; set;}
    public required string ZipCode {get; set;}
}