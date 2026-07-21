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
    private readonly IPremiumPaymentRepository _premiumPaymentRepository;

    public PolicyService(
        IPolicyRepository policyRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IPremiumPaymentRepository premiumPaymentRepository)
    {
        _policyRepository = policyRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _premiumPaymentRepository = premiumPaymentRepository;
    }


    public async Task<PoliciesDto> GetPoliciesAsync(Guid userId)
    {
        Customer customer = await GetCustomerOrThrow(userId);

        List<Policy> policies = await _policyRepository.GetByCustomerIdAsync(customer.Id);

        await CloseExpiredPoliciesAsync(policies);

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

        await CloseExpiredPoliciesAsync([policy]);

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


    public async Task<PolicyDto> RenewPolicyAsync(Guid userId, Guid policyId)
    {
        Customer customer = await GetCustomerOrThrow(userId);

        Policy policy = await _policyRepository.GetByIdForCustomerAsync(policyId, customer.Id)
            ?? throw new NotFoundException("Policy not found.");

        if (policy.Status == PolicyStatus.Cancelled)
            throw new ConflictException("A cancelled policy cannot be renewed.");

        if (await _policyRepository.HasRenewalAsync(policy.Id))
            throw new ConflictException("This policy has already been renewed.");

        List<Product> products = await _productRepository.GetByIdsAsync([policy.ProductId]);
        Product product = products.FirstOrDefault()
            ?? throw new ConflictException("The product for this policy is no longer available.");

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Renewing early carries straight on from the old end date, so there is
        // no gap in cover. If the policy already ended, cover restarts today.
        DateOnly startDate = policy.EndDate > today ? policy.EndDate : today;
        int termYears = Math.Max(1, (int)product.PolicyTermYears);

        Policy renewal = new()
        {
            PolicyNumber = $"POL-{DateTime.UtcNow:yyyyMMddHHmmss}",
            PreviousPolicyId = policy.Id,
            QuoteId = policy.QuoteId,
            ProductId = policy.ProductId,
            CustomerId = policy.CustomerId,
            InsuranceAgentId = policy.InsuranceAgentId,
            UnderWriterId = policy.UnderWriterId,
            CoverageAmount = policy.CoverageAmount,
            PremiumAmount = policy.PremiumAmount,
            QuoteDate = policy.QuoteDate,
            IssueDate = today,
            StartDate = startDate,
            EndDate = startDate.AddYears(termYears),
            Status = PolicyStatus.Active
        };

        await _policyRepository.AddAsync(renewal);

        // Start the renewed policy's premium schedule, same as a new policy.
        await _premiumPaymentRepository.AddAsync(new PremiumPayment
        {
            PolicyId = renewal.Id,
            InstallmentNumber = "1",
            PremiumAmount = renewal.PremiumAmount,
            Frequency = PremiumFrequency.monthly,
            PenaltyAmount = 0,
            DueDate = startDate.AddDays(30),
            PaymentStatus = PremiumPaymentStatus.Pending
        });

        // The old policy is left alone - it closes by itself on its end date
        // (see CloseExpiredPoliciesAsync), so early renewals keep cover running.
        await _policyRepository.SaveChangesAsync();

        return PolicyMapper.ToDto(renewal, product.Name);
    }


    // ---- helpers ----

    // Policy closure: once the term is over an active policy becomes Expired.
    // Done when policies are read, so no background job is needed.
    private async Task CloseExpiredPoliciesAsync(List<Policy> policies)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        bool anyClosed = false;

        foreach (Policy policy in policies)
        {
            if (policy.Status == PolicyStatus.Active && policy.EndDate < today)
            {
                policy.Status = PolicyStatus.Expired;
                anyClosed = true;
            }
        }

        if (anyClosed)
            await _policyRepository.SaveChangesAsync();
    }


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
