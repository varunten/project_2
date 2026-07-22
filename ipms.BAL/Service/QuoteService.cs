using IPMS.BAL.IService;
using IPMS.DAL.IRepository;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;
using IPMS.DTO.Enum;
using IPMS.DTO.Exceptions;

namespace IPMS.BAL.Service;


public class QuoteService : IQuoteService
{
    private readonly IQuoteRepository _quoteRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IPremiumPaymentRepository _premiumPaymentRepository;

    public QuoteService(
        IQuoteRepository quoteRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IPolicyRepository policyRepository,
        IPremiumPaymentRepository premiumPaymentRepository)
    {
        _quoteRepository = quoteRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _policyRepository = policyRepository;
        _premiumPaymentRepository = premiumPaymentRepository;
    }


    public async Task<QuoteDto> CreateQuoteAsync(Guid userId, CreateQuoteDto payload)
    {
        Customer customer = await GetCustomerOrThrow(userId);

        Product product = await _productRepository.GetByIdAsync(payload.ProductId)
            ?? throw new NotFoundException("Product not found.");

        decimal coverageAmount = payload.CoverageAmount ?? product.CoverageAmount;

        if (coverageAmount <= 0)
            throw new ValidationException("Coverage amount must be greater than zero.");

        // Simple premium rule: scale the base premium by requested coverage.
        decimal premiumAmount = product.CoverageAmount > 0
            ? Math.Round(product.BasePremium * (coverageAmount / product.CoverageAmount), 2)
            : product.BasePremium;

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        Quote quote = new()
        {
            QuoteNumber = $"QUO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ProductId = product.Id,
            CustomerId = customer.Id,
            CoverageAmount = coverageAmount,
            PremiumAmount = premiumAmount,
            QuoteDate = today,
            ValidUntil = today.AddDays(30),
            Status = QuoteStatus.Requested
        };

        await _quoteRepository.AddAsync(quote);
        await _quoteRepository.SaveChangesAsync();

        return MapToDto(quote, product.Name);
    }


    public async Task<QuotesDto> GetQuotesAsync(Guid userId)
    {
        Customer customer = await GetCustomerOrThrow(userId);

        List<Quote> quotes = await _quoteRepository.GetByCustomerIdAsync(customer.Id);

        Dictionary<Guid, string> productNames = await GetProductNames(quotes.Select(q => q.ProductId));

        List<QuoteDto> dtos = quotes
            .Select(q => MapToDto(q, productNames.GetValueOrDefault(q.ProductId, "")))
            .ToList();

        return new QuotesDto
        {
            Total = (ulong)dtos.Count,
            Quotes = dtos
        };
    }


    public async Task<QuoteDto> GetQuoteByIdAsync(Guid userId, Guid quoteId)
    {
        Customer customer = await GetCustomerOrThrow(userId);

        Quote quote = await _quoteRepository.GetByIdForCustomerAsync(quoteId, customer.Id)
            ?? throw new NotFoundException("Quote not found.");

        string productName = await GetProductName(quote.ProductId);

        return MapToDto(quote, productName);
    }


    public async Task<QuoteDto> AcceptQuoteAsync(Guid userId, Guid quoteId)
    {
        Customer customer = await GetCustomerOrThrow(userId);

        Quote quote = await _quoteRepository.GetByIdForCustomerAsync(quoteId, customer.Id)
            ?? throw new NotFoundException("Quote not found.");

        if (quote.Status != QuoteStatus.Requested)
            throw new ConflictException("Only requested quotes can be accepted.");

        if (IsExpired(quote))
        {
            quote.Status = QuoteStatus.Expired;
            await _quoteRepository.SaveChangesAsync();
            throw new ConflictException("This quote has expired.");
        }

        quote.Status = QuoteStatus.AcceptedByCustomer;
        await _quoteRepository.SaveChangesAsync();

        string productName = await GetProductName(quote.ProductId);
        return MapToDto(quote, productName);
    }


    public async Task<QuoteDto> GetQuoteForReviewAsync(Guid quoteId)
    {
        Quote quote = await _quoteRepository.GetByIdAsync(quoteId)
            ?? throw new NotFoundException("Quote not found.");

        string productName = await GetProductName(quote.ProductId);
        return MapToDto(quote, productName);
    }


    public async Task<QuotesDto> GetPendingQuotesAsync()
    {
        List<Quote> quotes = await _quoteRepository.GetPendingForReviewAsync();

        Dictionary<Guid, string> productNames = await GetProductNames(quotes.Select(q => q.ProductId));

        List<QuoteDto> dtos = quotes
            .Select(q => MapToDto(q, productNames.GetValueOrDefault(q.ProductId, "")))
            .ToList();

        return new QuotesDto
        {
            Total = (ulong)dtos.Count,
            Quotes = dtos
        };
    }


    public async Task<QuoteDto> CancelQuoteAsync(Guid userId, Guid quoteId)
    {
        Customer customer = await GetCustomerOrThrow(userId);

        Quote quote = await _quoteRepository.GetByIdForCustomerAsync(quoteId, customer.Id)
            ?? throw new NotFoundException("Quote not found.");

        // Only a quote that hasn't been decided yet can be withdrawn.
        if (quote.Status is not (QuoteStatus.Requested or QuoteStatus.AcceptedByCustomer))
            throw new ConflictException("This quote can no longer be cancelled.");

        quote.Status = QuoteStatus.Rejected;
        await _quoteRepository.SaveChangesAsync();

        string productName = await GetProductName(quote.ProductId);
        return MapToDto(quote, productName);
    }


    public async Task<PolicyDto> ApproveQuoteAsync(Guid underwriterId, Guid quoteId)
    {
        Quote quote = await _quoteRepository.GetByIdAsync(quoteId)
            ?? throw new NotFoundException("Quote not found.");

        if (quote.Status != QuoteStatus.AcceptedByCustomer)
            throw new ConflictException("Only quotes accepted by the customer can be approved.");

        if (IsExpired(quote))
        {
            quote.Status = QuoteStatus.Expired;
            await _quoteRepository.SaveChangesAsync();
            throw new ConflictException("This quote has expired.");
        }

        List<Product> products = await _productRepository.GetByIdsAsync(new List<Guid> { quote.ProductId });
        Product product = products.FirstOrDefault()
            ?? throw new ConflictException("The product for this quote no longer exists.");

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int termYears = Math.Max(1, (int)product.PolicyTermYears);

        Policy policy = new()
        {
            PolicyNumber = $"POL-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ProductId = quote.ProductId,
            CustomerId = quote.CustomerId,
            QuoteId = quote.Id,
            // The customer bought through the agent who created the product.
            InsuranceAgentId = product.InsuranceAgentId,
            UnderWriterId = underwriterId,
            CoverageAmount = quote.CoverageAmount,
            PremiumAmount = quote.PremiumAmount,
            QuoteDate = quote.QuoteDate,
            IssueDate = today,
            StartDate = today,
            EndDate = today.AddYears(termYears),
            Status = PolicyStatus.Active
        };

        quote.Status = QuoteStatus.Approved;
        quote.UnderwriterId = underwriterId;
        quote.ApprovedDate = today;

        await _policyRepository.AddAsync(policy);

        // Issuing a policy also starts its premium schedule, so the customer
        // has something to pay straight away.
        PremiumPayment firstInstallment = new()
        {
            PolicyId = policy.Id,
            InstallmentNumber = "1",
            PremiumAmount = policy.PremiumAmount,
            Frequency = PremiumFrequency.monthly,
            PenaltyAmount = 0,
            DueDate = today.AddDays(30),
            PaymentStatus = PremiumPaymentStatus.Pending
        };

        await _premiumPaymentRepository.AddAsync(firstInstallment);

        // Single SaveChanges commits the new policy, the installment and the
        // quote update (all repositories share the same DbContext).
        await _quoteRepository.SaveChangesAsync();

        return PolicyMapper.ToDto(policy, product.Name);
    }


    public async Task<QuoteDto> RejectQuoteAsync(Guid underwriterId, Guid quoteId)
    {
        Quote quote = await _quoteRepository.GetByIdAsync(quoteId)
            ?? throw new NotFoundException("Quote not found.");

        if (quote.Status != QuoteStatus.AcceptedByCustomer)
            throw new ConflictException("Only quotes accepted by the customer can be rejected.");

        quote.Status = QuoteStatus.Rejected;
        quote.UnderwriterId = underwriterId;
        await _quoteRepository.SaveChangesAsync();

        string productName = await GetProductName(quote.ProductId);
        return MapToDto(quote, productName);
    }


    // ---- helpers ----

    private async Task<Customer> GetCustomerOrThrow(Guid userId)
    {
        return await _customerRepository.GetActiveByUserIdAsync(userId)
            ?? throw new BadRequestException("You must create a customer profile first.");
    }

    private static bool IsExpired(Quote quote)
    {
        return quote.ValidUntil < DateOnly.FromDateTime(DateTime.UtcNow);
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

    private static QuoteDto MapToDto(Quote q, string productName)
    {
        return new QuoteDto
        {
            Id = q.Id,
            QuoteNumber = q.QuoteNumber,
            ProductName = productName,
            CoverageAmount = q.CoverageAmount,
            PremiumAmount = q.PremiumAmount,
            QuoteDate = q.QuoteDate,
            ValidUntil = q.ValidUntil,
            Status = q.Status,
            CreatedAt = q.CreatedAt
        };
    }
}
