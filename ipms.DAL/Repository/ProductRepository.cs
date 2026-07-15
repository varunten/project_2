using IPMS.DAL.Data;
using IPMS.DAL.IRepository;
using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPMS.DAL.Repository;


public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<List<Product>> GetAllActiveAsync()
    {
        return await _context.Products
            .Where(p => p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .ToListAsync();
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
