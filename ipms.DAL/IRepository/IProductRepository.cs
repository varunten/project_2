using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;

namespace IPMS.DAL.IRepository;


public interface IProductRepository
{
    // Active products matching the filter/sort/paging options. When
    // customerAge is supplied, only products whose [MinAge, MaxAge] range
    // covers that age are returned. Also reports the total (pre-paging) count.
    Task<(List<Product> Items, int Total)> QueryActiveAsync(ProductQueryDto query, int? customerAge);

    Task<Product?> GetByIdAsync(Guid id);

    // Lookup by ids ignoring soft-delete, for resolving names on historical
    // records (a quote/policy may reference a product that was later removed).
    Task<List<Product>> GetByIdsAsync(List<Guid> ids);

    Task<bool> ExistsByNameAsync(string name);

    Task AddAsync(Product product);

    Task SaveChangesAsync();
}
