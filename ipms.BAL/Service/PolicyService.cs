using IPMS.BAL.IService;
using IPMS.DAL.IRepository;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;
using IPMS.DTO.Enum;
using IPMS.DTO.Exceptions;

namespace IPMS.BAL.Service;


public class PolicyService : IPolicyService
{
    private readonly IPolicyRepository _policyRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;

    public PolicyService(
        IPolicyRepository policyRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository)
    {
        _policyRepository = policyRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
    }


    public async Task<PoliciesDto> GetPoliciesAsync(Guid userId)
    {
        Customer customer = await GetCustomerOrThrow(userId);

        List<Policy> policies = await _policyRepository.GetByCustomerIdAsync(customer.Id);

        Dictionary<Guid, string> productNames = await GetProductNames(policies.Select(p => p.ProductId));

        List<PolicyDto> dtos = policies
            .Select(p => PolicyMapper.ToDto(p, productNames.GetValueOrDefault(p.ProductId, "")))
            .ToList();

        return new PoliciesDto
        {
            Total = (ulong)dtos.Count,
            Policies = dtos
        };
    }


    public async Task<PolicyDto> GetPolicyByIdAsync(Guid userId, Guid policyId)
    {
        Customer customer = await GetCustomerOrThrow(userId);

        Policy policy = await _policyRepository.GetByIdForCustomerAsync(policyId, customer.Id)
            ?? throw new NotFoundException("Policy not found.");

        string productName = await GetProductName(policy.ProductId);

        return PolicyMapper.ToDto(policy, productName);
    }


    public async Task<PolicyDto> CancelPolicyAsync(Guid userId, Guid policyId, CancelPolicyDto payload)
    {
        Customer customer = await GetCustomerOrThrow(userId);

        Policy policy = await _policyRepository.GetByIdForCustomerAsync(policyId, customer.Id)
            ?? throw new NotFoundException("Policy not found.");

        if (policy.Status != PolicyStatus.Active)
            throw new ConflictException("Only active policies can be cancelled.");

        policy.Status = PolicyStatus.Cancelled;
        policy.CancellationDate = DateTimeOffset.UtcNow;
        policy.CancellationReason = payload.CancellationReason;

        await _policyRepository.SaveChangesAsync();

        string productName = await GetProductName(policy.ProductId);

        return PolicyMapper.ToDto(policy, productName);
    }


    // ---- helpers ----

    private async Task<Customer> GetCustomerOrThrow(Guid userId)
    {
        return await _customerRepository.GetActiveByUserIdAsync(userId)
            ?? throw new BadRequestException("You must create a customer profile first.");
    }

    private async Task<string> GetProductName(Guid productId)
    {
        Dictionary<Guid, string> names = await GetProductNames(new[] { productId });
        return names.GetValueOrDefault(productId, "");
    }

    private async Task<Dictionary<Guid, string>> GetProductNames(IEnumerable<Guid> productIds)
    {
        List<Guid> ids = productIds.Distinct().ToList();
        List<Product> products = await _productRepository.GetByIdsAsync(ids);
        return products.ToDictionary(p => p.Id, p => p.Name);
    }
}
