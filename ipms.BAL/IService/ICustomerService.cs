using IPMS.DTO.Dtos;

namespace IPMS.BAL.IService;


public interface ICustomerService
{
    // The signed-in user creates their own customer profile.
    Task<CustomerDto> CreateCustomerAsync(Guid userId, CreateCustomerDto payload);

    Task<CustomersDto> GetCustomersAsync();

    Task<CustomerDto> GetCustomerByIdAsync(Guid customerId);

    Task<CustomerDto> UpdateCustomerAsync(Guid customerId, UpdateCustomerDto payload);

    Task DeleteCustomerAsync(Guid customerId);
}
