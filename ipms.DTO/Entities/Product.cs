using IPMS.DTO.Enum;

namespace IPMS.DTO.Entities;





public class Product: BaseEntity
{
    // The insurance agent who created this product. Copied onto any policy sold
    // from it, so we know which agent the customer bought through.
    public Guid? InsuranceAgentId {get; set;}
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