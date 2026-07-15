using IPMS.DTO.Entities;

namespace IPMS.DAL.IRepository;


public interface IProductRepository
{
    Task<List<Product>> GetAllActiveAsync();

    Task<Product?> GetByIdAsync(Guid id);

    // Lookup by ids ignoring soft-delete, for resolving names on historical
    // records (a quote/policy may reference a product that was later removed).
    Task<List<Product>> GetByIdsAsync(List<Guid> ids);

    Task<bool> ExistsByNameAsync(string name);

    Task AddAsync(Product product);

    Task SaveChangesAsync();
}
