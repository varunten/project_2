using IPMS.BAL.IService;
using IPMS.DAL.IRepository;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;
using IPMS.DTO.Exceptions;

namespace IPMS.BAL.Service;


public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }


    public async Task<ProductsDto> GetProductsAsync()
    {
        List<Product> products = await _repository.GetAllActiveAsync();

        List<ProductDto> dtos = products.Select(MapToDto).ToList();

        return new ProductsDto
        {
            Total = (ulong)dtos.Count,
            Products = dtos
        };
    }


    public async Task<ProductDto> GetProductByIdAsync(Guid id)
    {
        Product product = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product not found.");

        return MapToDto(product);
    }


    public async Task<ProductDto> CreateProductAsync(Guid agentId, CreateProductDto payload)
    {
        if (payload.MinAge > payload.MaxAge)
            throw new ValidationException("MinAge cannot be greater than MaxAge.");

        if (await _repository.ExistsByNameAsync(payload.Name))
            throw new ConflictException("A product with this name already exists.");

        Product product = new()
        {
            InsuranceAgentId = agentId,
            Name = payload.Name,
            Type = payload.Type,
            CoverageAmount = payload.CoverageAmount,
            BasePremium = payload.BasePremium,
            MinAge = payload.MinAge,
            MaxAge = payload.MaxAge,
            PolicyTermYears = payload.PolicyTermYears,
            Description = payload.Description
        };

        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();

        return MapToDto(product);
    }


    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto payload)
    {
        Product product = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product not found.");

        if (payload.Name is not null)
            product.Name = payload.Name;

        if (payload.Type.HasValue)
            product.Type = payload.Type.Value;

        if (payload.CoverageAmount.HasValue)
            product.CoverageAmount = payload.CoverageAmount.Value;

        if (payload.BasePremium.HasValue)
            product.BasePremium = payload.BasePremium.Value;

        if (payload.MinAge.HasValue)
            product.MinAge = payload.MinAge.Value;

        if (payload.MaxAge.HasValue)
            product.MaxAge = payload.MaxAge.Value;

        if (payload.PolicyTermYears.HasValue)
            product.PolicyTermYears = payload.PolicyTermYears.Value;

        if (payload.Description is not null)
            product.Description = payload.Description;

        // IsActive is a friendly view over the soft-delete flag.
        if (payload.IsActive.HasValue)
            product.DeletedAt = payload.IsActive.Value ? null : DateTimeOffset.UtcNow;

        if (product.MinAge > product.MaxAge)
            throw new ValidationException("MinAge cannot be greater than MaxAge.");

        await _repository.SaveChangesAsync();

        return MapToDto(product);
    }


    public async Task DeleteProductAsync(Guid id)
    {
        Product product = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product not found.");

        product.DeletedAt = DateTimeOffset.UtcNow;

        await _repository.SaveChangesAsync();
    }


    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Type = p.Type,
            CoverageAmount = p.CoverageAmount,
            BasePremium = p.BasePremium,
            MinAge = p.MinAge,
            MaxAge = p.MaxAge,
            PolicyTermYears = p.PolicyTermYears,
            Description = p.Description,
            IsActive = p.DeletedAt == null,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}
