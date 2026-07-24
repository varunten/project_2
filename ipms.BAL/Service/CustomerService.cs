using System.Security.Cryptography;
using System.Text;
using IPMS.BAL.IService;
using IPMS.DAL.IRepository;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;
using IPMS.DTO.Exceptions;

namespace IPMS.BAL.Service;


public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }


    public async Task<CustomerDto> CreateCustomerAsync(Guid userId, CreateCustomerDto payload)
    {
        User user = await _repository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException("User not found.");

        Customer? existing = await _repository.GetActiveByUserIdAsync(userId);
        if (existing is not null)
            throw new ConflictException("A customer profile already exists for this user.");

        Customer customer = new()
        {
            UserId = userId,
            DateOfBirth = payload.DateOfBirth,
            Gender = payload.Gender,
            MaritalStatus = payload.MaritalStatus,
            SSNHash = HashSsn(payload.SSN)
        };

        await _repository.AddCustomerAsync(customer);
        await _repository.SaveChangesAsync();

        CustomerAddress address = new()
        {
            CustomerId = customer.Id,
            HouseNumber = payload.Address.HouseNumber,
            StreetNumber = payload.Address.StreetNumber,
            StreetName = payload.Address.StreetName,
            StreetSuffix = payload.Address.StreetSuffix,
            City = payload.Address.City,
            State = payload.Address.State,
            ZipCode = payload.Address.ZipCode
        };

        await _repository.AddAddressAsync(address);
        await _repository.SaveChangesAsync();

        return MapToDto(customer, user, address);
    }


    public async Task<CustomerDto> GetMyProfileAsync(Guid userId)
    {
        Customer customer = await _repository.GetActiveByUserIdAsync(userId)
            ?? throw new NotFoundException("You have not created a customer profile yet.");

        return await GetCustomerByIdAsync(customer.Id);
    }


    public async Task<CustomerDto> UpdateMyProfileAsync(Guid userId, UpdateCustomerDto payload)
    {
        Customer customer = await _repository.GetActiveByUserIdAsync(userId)
            ?? throw new NotFoundException("You have not created a customer profile yet.");

        // Email is the account's sign-in identity, so a customer cannot change
        // it from their own profile. The form shows it read-only; dropping it
        // here means a hand-crafted request cannot get around that either.
        // Staff can still correct an address via UpdateCustomerAsync.
        payload.Email = null;

        return await UpdateCustomerAsync(customer.Id, payload);
    }


    public async Task<CustomersDto> GetCustomersAsync()
    {
        List<Customer> customers = await _repository.GetAllActiveAsync();

        List<User> users = await _repository.GetUsersByIdsAsync(
            customers.Select(c => c.UserId).ToList());

        List<CustomerAddress> addresses = await _repository.GetAddressesByCustomerIdsAsync(
            customers.Select(c => c.Id).ToList());

        Dictionary<Guid, User> userById = users.ToDictionary(u => u.Id);
        Dictionary<Guid, CustomerAddress> addressByCustomerId =
            addresses.GroupBy(a => a.CustomerId)
                     .ToDictionary(g => g.Key, g => g.First());

        List<CustomerDto> dtos = customers
            .Where(c => userById.ContainsKey(c.UserId))
            .Select(c => MapToDto(
                c,
                userById[c.UserId],
                addressByCustomerId.GetValueOrDefault(c.Id)))
            .ToList();

        return new CustomersDto
        {
            Total = (ulong)dtos.Count,
            Customers = dtos
        };
    }


    public async Task<CustomerDto> GetCustomerByIdAsync(Guid customerId)
    {
        Customer customer = await _repository.GetActiveByIdAsync(customerId)
            ?? throw new NotFoundException("Customer not found.");

        User user = await _repository.GetUserByIdAsync(customer.UserId)
            ?? throw new NotFoundException("Customer's user account not found.");

        CustomerAddress? address = await _repository.GetAddressByCustomerIdAsync(customer.Id);

        return MapToDto(customer, user, address);
    }


    public async Task<CustomerDto> UpdateCustomerAsync(Guid customerId, UpdateCustomerDto payload)
    {
        Customer customer = await _repository.GetActiveByIdAsync(customerId)
            ?? throw new NotFoundException("Customer not found.");

        User user = await _repository.GetUserByIdAsync(customer.UserId)
            ?? throw new NotFoundException("Customer's user account not found.");

        CustomerAddress? address = await _repository.GetAddressByCustomerIdAsync(customer.Id);

        // User fields
        if (payload.FirstName is not null) user.FirstName = payload.FirstName;
        if (payload.MiddleName is not null) user.MiddleName = payload.MiddleName;
        if (payload.LastName is not null) user.LastName = payload.LastName;
        if (payload.Email is not null) user.Email = AuthService.NormalizeEmail(payload.Email);
        if (payload.PhoneNumber is not null) user.PhoneNumber = payload.PhoneNumber;

        // Customer fields
        if (payload.DateOfBirth.HasValue) customer.DateOfBirth = payload.DateOfBirth.Value;
        if (payload.Gender.HasValue) customer.Gender = payload.Gender.Value;
        if (payload.MaritalStatus.HasValue) customer.MaritalStatus = payload.MaritalStatus.Value;

        // Address fields
        if (payload.Address is not null && address is not null)
        {
            if (payload.Address.HouseNumber.HasValue) address.HouseNumber = payload.Address.HouseNumber.Value;
            if (payload.Address.StreetNumber.HasValue) address.StreetNumber = payload.Address.StreetNumber.Value;
            if (payload.Address.StreetName is not null) address.StreetName = payload.Address.StreetName;
            if (payload.Address.StreetSuffix is not null) address.StreetSuffix = payload.Address.StreetSuffix;
            if (payload.Address.City is not null) address.City = payload.Address.City;
            if (payload.Address.State is not null) address.State = payload.Address.State;
            if (payload.Address.ZipCode is not null) address.ZipCode = payload.Address.ZipCode;
        }

        await _repository.SaveChangesAsync();

        return MapToDto(customer, user, address);
    }


    public async Task DeleteCustomerAsync(Guid customerId)
    {
        Customer customer = await _repository.GetActiveByIdAsync(customerId)
            ?? throw new NotFoundException("Customer not found.");

        customer.DeletedAt = DateTimeOffset.UtcNow;

        await _repository.SaveChangesAsync();
    }


    // SSNs are never stored in plain text - keep only a one-way hash.
    private static string HashSsn(string ssn)
    {
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(ssn)));
    }


    private static CustomerDto MapToDto(Customer c, User u, CustomerAddress? a)
    {
        return new CustomerDto
        {
            Id = c.Id,
            FirstName = u.FirstName,
            MiddleName = u.MiddleName,
            LastName = u.LastName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            DateOfBirth = c.DateOfBirth,
            Gender = c.Gender,
            MaritalStatus = c.MaritalStatus,
            Address = new CustomerAddressDto
            {
                HouseNumber = a?.HouseNumber,
                StreetNumber = a?.StreetNumber,
                StreetName = a?.StreetName,
                StreetSuffix = a?.StreetSuffix,
                City = a?.City ?? string.Empty,
                State = a?.State ?? string.Empty,
                ZipCode = a?.ZipCode ?? string.Empty
            },
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
}
