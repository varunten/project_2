using IPMS.BAL.IService;
using IPMS.DAL.IRepository;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;
using IPMS.DTO.Enum;
using IPMS.DTO.Exceptions;

namespace IPMS.BAL.Service;


public class PremiumPaymentService : IPremiumPaymentService
{
    private readonly IPremiumPaymentRepository _repository;
    private readonly IPolicyRepository _policyRepository;
    private readonly ICustomerRepository _customerRepository;

    public PremiumPaymentService(
        IPremiumPaymentRepository repository,
        IPolicyRepository policyRepository,
        ICustomerRepository customerRepository)
    {
        _repository = repository;
        _policyRepository = policyRepository;
        _customerRepository = customerRepository;
    }


    public async Task<PremiumPaymentsDto> GetPolicyPaymentsAsync(Guid userId, Guid policyId, bool isStaff)
    {
        Policy policy = await _policyRepository.GetByIdAsync(policyId)
            ?? throw new NotFoundException("Policy not found.");

        // Customers may only look at their own policies.
        if (!isStaff)
        {
            Customer customer = await _customerRepository.GetActiveByUserIdAsync(userId)
                ?? throw new BadRequestException("You must create a customer profile first.");

            if (policy.CustomerId != customer.Id)
                throw new ForbiddenException("You can only view payments for your own policies.");
        }

        List<PremiumPayment> payments = await _repository.GetByPolicyIdAsync(policyId);

        List<PremiumPaymentDto> dtos = payments.Select(MapToDto).ToList();

        return new PremiumPaymentsDto
        {
            Total = (ulong)dtos.Count,
            Payments = dtos
        };
    }


    public async Task<PremiumPaymentDto> CreatePremiumPaymentAsync(CreatePremiumPaymentDto payload)
    {
        Policy _ = await _policyRepository.GetByIdAsync(payload.PolicyId)
            ?? throw new NotFoundException("Policy not found.");

        if (payload.PremiumAmount <= 0)
            throw new ValidationException("Premium amount must be greater than zero.");

        PremiumPayment payment = new()
        {
            PolicyId = payload.PolicyId,
            InstallmentNumber = payload.InstallmentNumber,
            PremiumAmount = payload.PremiumAmount,
            Frequency = payload.Frequency,
            PenaltyAmount = payload.PenaltyAmount,
            DueDate = payload.DueDate,
            PaymentStatus = PremiumPaymentStatus.Pending
        };

        await _repository.AddAsync(payment);
        await _repository.SaveChangesAsync();

        return MapToDto(payment);
    }


    public async Task<PremiumPaymentDto> PayPremiumAsync(Guid userId, Guid paymentId, PayPremiumDto payload)
    {
        PremiumPayment payment = await _repository.GetByIdAsync(paymentId)
            ?? throw new NotFoundException("Premium payment not found.");

        if (payment.PaymentStatus == PremiumPaymentStatus.Success)
            throw new ConflictException("This installment has already been paid.");

        // Make sure the payment belongs to a policy owned by this customer.
        Customer customer = await _customerRepository.GetActiveByUserIdAsync(userId)
            ?? throw new BadRequestException("You must create a customer profile first.");

        Policy policy = await _policyRepository.GetByIdAsync(payment.PolicyId)
            ?? throw new NotFoundException("Policy not found.");

        if (policy.CustomerId != customer.Id)
            throw new ForbiddenException("You can only pay premiums for your own policies.");

        decimal amountDue = payment.PremiumAmount + payment.PenaltyAmount;
        if (payload.AmountPaid < amountDue)
            throw new ValidationException($"Amount paid must be at least {amountDue}.");

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        payment.TotalPaid = payload.AmountPaid;
        payment.PaymentMethod = payload.PaymentMethod;
        payment.PaidDate = today;
        payment.PaymentStatus = today > payment.DueDate
            ? PremiumPaymentStatus.Late
            : PremiumPaymentStatus.Success;

        await _repository.SaveChangesAsync();

        return MapToDto(payment);
    }


    public async Task<PremiumPaymentDto> UpdatePremiumPaymentAsync(Guid paymentId, UpdatePremiumPaymentDto payload)
    {
        PremiumPayment payment = await _repository.GetByIdAsync(paymentId)
            ?? throw new NotFoundException("Premium payment not found.");

        if (payload.PaymentStatus.HasValue) payment.PaymentStatus = payload.PaymentStatus.Value;
        if (payload.PaymentMethod.HasValue) payment.PaymentMethod = payload.PaymentMethod.Value;
        if (payload.PenaltyAmount.HasValue) payment.PenaltyAmount = payload.PenaltyAmount.Value;

        await _repository.SaveChangesAsync();

        return MapToDto(payment);
    }


    private static PremiumPaymentDto MapToDto(PremiumPayment p)
    {
        return new PremiumPaymentDto
        {
            Id = p.Id,
            PolicyId = p.PolicyId,
            InstallmentNumber = p.InstallmentNumber,
            PremiumAmount = p.PremiumAmount,
            PenaltyAmount = p.PenaltyAmount,
            TotalPaid = p.TotalPaid,
            Frequency = p.Frequency,
            DueDate = p.DueDate,
            PaidDate = p.PaidDate,
            PaymentStatus = p.PaymentStatus,
            PaymentMethod = p.PaymentMethod,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}
