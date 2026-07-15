using IPMS.DTO.Dtos;

namespace IPMS.BAL.IService;


public interface IProductService
{
    Task<ProductsDto> GetProductsAsync();

    Task<ProductDto> GetProductByIdAsync(Guid id);

    Task<ProductDto> CreateProductAsync(CreateProductDto payload);

    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto payload);

    Task DeleteProductAsync(Guid id);
}
