using IPMS.DTO.Entities;

namespace IPMS.DAL.IRepository;


public interface ICustomerRepository
{
    Task<User?> GetUserByIdAsync(Guid userId);

    Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds);

    // Active (not soft-deleted) customer profiles.
    Task<Customer?> GetActiveByIdAsync(Guid customerId);

    Task<Customer?> GetActiveByUserIdAsync(Guid userId);

    Task<List<Customer>> GetAllActiveAsync();

    Task<CustomerAddress?> GetAddressByCustomerIdAsync(Guid customerId);

    Task<List<CustomerAddress>> GetAddressesByCustomerIdsAsync(List<Guid> customerIds);

    Task AddCustomerAsync(Customer customer);

    Task AddAddressAsync(CustomerAddress address);

    Task SaveChangesAsync();
}
