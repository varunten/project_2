using IPMS.DTO.Enum;

namespace IPMS.DTO.Entities;





public class Product: BaseEntity
{
    public required string Name {get; set;}
    public required ProductType Type {get; set;}
    public required decimal CoverageAmount {get; set;}
    public required decimal BasePremium {get; set;}
    public required byte MinAge {get; set;}
    public required byte MaxAge {get; set;}
    public required decimal PolicyTermYears {get; set;}
    public string? Description {get; set;}
    public DateTimeOffset? DeletedAt {get; set;}
}