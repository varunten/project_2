
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


// How the catalogue should be filtered, sorted and paged. Bound straight from
// the query string on GET /api/product, so every field is optional and has a
// sensible default applied in the service.
public class ProductQueryDto
{
    // Case-insensitive match on the product name.
    public string? Search {get; set;}

    // Restrict to a single product type.
    public ProductType? Type {get; set;}

    // One of: name | premium | coverage | created. Defaults to name.
    public string? SortBy {get; set;}

    // asc | desc. Defaults to asc.
    public string? SortDir {get; set;}

    // 1-based page number and page size (both clamped in the service).
    public int Page {get; set;} = 1;
    public int PageSize {get; set;} = 10;
}


public class ProductsDto
{
    public required ulong Total {get; set;}
    public required List<ProductDto> Products {get; set;}

    // Paging metadata, so the caller can render page controls. The effective
    // (clamped) page and size are echoed back here.
    public int Page {get; set;}
    public int PageSize {get; set;}
    public int TotalPages {get; set;}
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