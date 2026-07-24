using IPMS.DAL.Data;
using IPMS.DAL.IRepository;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;
using IPMS.DTO.Enum;
using Microsoft.EntityFrameworkCore;

namespace IPMS.DAL.Repository;


public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<(List<Product> Items, int Total)> QueryActiveAsync(
        ProductQueryDto query, int? customerAge)
    {
        IQueryable<Product> q = _context.Products.Where(p => p.DeletedAt == null);

        // Age eligibility: only products the customer's age falls within.
        if (customerAge is int age)
            q = q.Where(p => p.MinAge <= age && p.MaxAge >= age);

        // Filtering.
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            q = q.Where(p => p.Name.Contains(term));
        }

        if (query.Type is ProductType type)
            q = q.Where(p => p.Type == type);

        // Count before paging so the caller knows the full result size.
        int total = await q.CountAsync();

        // Sorting. Name is the default and also the tie-breaker for a stable order.
        bool desc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        q = query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "premium" => desc
                ? q.OrderByDescending(p => p.BasePremium).ThenBy(p => p.Name)
                : q.OrderBy(p => p.BasePremium).ThenBy(p => p.Name),
            "coverage" => desc
                ? q.OrderByDescending(p => p.CoverageAmount).ThenBy(p => p.Name)
                : q.OrderBy(p => p.CoverageAmount).ThenBy(p => p.Name),
            "created" => desc
                ? q.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Name)
                : q.OrderBy(p => p.CreatedAt).ThenBy(p => p.Name),
            _ => desc
                ? q.OrderByDescending(p => p.Name)
                : q.OrderBy(p => p.Name),
        };

        // Paging.
        List<Product> items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, total);
    }


    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);
    }


    public async Task<List<Product>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }


    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Products
            .AnyAsync(p => p.Name == name && p.DeletedAt == null);
    }


    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
