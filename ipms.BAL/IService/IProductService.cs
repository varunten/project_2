using IPMS.DTO.Dtos;

namespace IPMS.BAL.IService;


public interface IProductService
{
    // Browse the catalogue. When the caller is a customer, results are limited
    // to products their current age is eligible for; staff see everything.
    Task<ProductsDto> GetProductsAsync(ProductQueryDto query, Guid userId, bool isCustomer);

    Task<ProductDto> GetProductByIdAsync(Guid id);

    // agentId = the insurance agent creating the product (recorded on it).
    Task<ProductDto> CreateProductAsync(Guid agentId, CreateProductDto payload);

    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto payload);

    Task DeleteProductAsync(Guid id);
}
