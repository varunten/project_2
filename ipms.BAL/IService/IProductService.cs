using IPMS.DTO.Dtos;

namespace IPMS.BAL.IService;


public interface IProductService
{
    Task<ProductsDto> GetProductsAsync();

    Task<ProductDto> GetProductByIdAsync(Guid id);

    // agentId = the insurance agent creating the product (recorded on it).
    Task<ProductDto> CreateProductAsync(Guid agentId, CreateProductDto payload);

    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto payload);

    Task DeleteProductAsync(Guid id);
}
