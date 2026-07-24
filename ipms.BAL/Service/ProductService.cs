using IPMS.BAL.IService;
using IPMS.DAL.IRepository;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;
using IPMS.DTO.Exceptions;

namespace IPMS.BAL.Service;


public class ProductService : IProductService
{
    // Page size is clamped to this range so a caller can't request the whole
    // table (or a nonsensical size) in one go.
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    private readonly IProductRepository _repository;
    private readonly ICustomerRepository _customerRepository;

    public ProductService(
        IProductRepository repository,
        ICustomerRepository customerRepository)
    {
        _repository = repository;
        _customerRepository = customerRepository;
    }


    public async Task<ProductsDto> GetProductsAsync(ProductQueryDto query, Guid userId, bool isCustomer)
    {
        Normalize(query);

        // Customers only see products their age is eligible for. A customer who
        // hasn't set up a profile yet has no known age, so the age filter is
        // skipped (they still can't request a quote until the profile exists).
        int? customerAge = null;
        if (isCustomer)
        {
            Customer? customer = await _customerRepository.GetActiveByUserIdAsync(userId);
            if (customer is not null)
                customerAge = AgeInYears(customer.DateOfBirth);
        }

        (List<Product> products, int total) = await _repository.QueryActiveAsync(query, customerAge);

        List<ProductDto> dtos = products.Select(MapToDto).ToList();

        return new ProductsDto
        {
            Total = (ulong)total,
            Products = dtos,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)query.PageSize)
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


    // Clamp paging to safe bounds so bad input can't skip the guard rails.
    private static void Normalize(ProductQueryDto query)
    {
        if (query.Page < 1)
            query.Page = 1;

        if (query.PageSize < 1)
            query.PageSize = DefaultPageSize;
        else if (query.PageSize > MaxPageSize)
            query.PageSize = MaxPageSize;
    }


    // Completed years between the date of birth and today (UTC).
    private static int AgeInYears(DateOnly dateOfBirth)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int age = today.Year - dateOfBirth.Year;

        // Not had this year's birthday yet? Then one fewer completed year.
        if (today < dateOfBirth.AddYears(age))
            age--;

        return age;
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
