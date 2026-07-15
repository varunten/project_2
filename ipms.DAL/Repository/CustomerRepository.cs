using IPMS.DAL.Data;
using IPMS.DAL.IRepository;
using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPMS.DAL.Repository;


public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
    }


    public async Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        return await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();
    }


    public async Task<Customer?> GetActiveByIdAsync(Guid customerId)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && c.DeletedAt == null);
    }


    public async Task<Customer?> GetActiveByUserIdAsync(Guid userId)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId && c.DeletedAt == null);
    }


    public async Task<List<Customer>> GetAllActiveAsync()
    {
        return await _context.Customers
            .Where(c => c.DeletedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }


    public async Task<CustomerAddress?> GetAddressByCustomerIdAsync(Guid customerId)
    {
        return await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.CustomerId == customerId);
    }


    public async Task<List<CustomerAddress>> GetAddressesByCustomerIdsAsync(List<Guid> customerIds)
    {
        return await _context.CustomerAddresses
            .Where(a => customerIds.Contains(a.CustomerId))
            .ToListAsync();
    }


    public async Task AddCustomerAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
    }


    public async Task AddAddressAsync(CustomerAddress address)
    {
        await _context.CustomerAddresses.AddAsync(address);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
