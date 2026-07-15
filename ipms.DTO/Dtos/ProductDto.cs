
using System.ComponentModel.DataAnnotations;
using IPMS.DTO.Enum;

namespace IPMS.DTO.Dtos;


public class ProductDto
{
    public required Guid Id {get; set;}
    public required string Name {get; set;}
    public required ProductType Type {get; set;}
    public required decimal CoverageAmount {get; set;}
    public required decimal BasePremium {get; set;}
    public required byte MinAge {get; set;}
    public required byte MaxAge {get; set;}
    public required decimal PolicyTermYears {get; set;}
    public string? Description {get; set;}
    public required bool IsActive {get; set;}
    public required DateTimeOffset CreatedAt {get; set;}
    public required DateTimeOffset UpdatedAt {get; set;}
}


public class CreateProductDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Product name is required.")]
    public required string Name {get; set;}

    public required ProductType Type {get; set;}

    [Range(1, 100000000, ErrorMessage = "Coverage amount must be greater than zero.")]
    public required decimal CoverageAmount {get; set;}

    [Range(1, 100000000, ErrorMessage = "Base premium must be greater than zero.")]
    public required decimal BasePremium {get; set;}

    [Range(0, 120, ErrorMessage = "Minimum age must be between 0 and 120.")]
    public required byte MinAge {get; set;}

    [Range(0, 120, ErrorMessage = "Maximum age must be between 0 and 120.")]
    public required byte MaxAge {get; set;}

    [Range(1, 100, ErrorMessage = "Policy term (years) must be between 1 and 100.")]
    public required decimal PolicyTermYears {get; set;}

    public string? Description {get; set;}
}


public class ProductsDto
{
    public required ulong Total {get; set;}
    public required List<ProductDto> Products {get; set;}
}

public class UpdateProductDto
{
    public string? Name {get; set;}
    public ProductType? Type {get; set;}
    public decimal? CoverageAmount {get; set;}
    public decimal? BasePremium {get; set;}
    public byte? MinAge {get; set;}
    public byte? MaxAge {get; set;}
    public decimal? PolicyTermYears {get; set;}
    public string? Description {get; set;}
    public bool? IsActive {get; set;}
}