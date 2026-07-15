using IPMS.DTO.Dtos;

namespace IPMS.BAL.IService;


public interface IPremiumPaymentService
{
    // Staff may view any policy's schedule; a customer only their own.
    Task<PremiumPaymentsDto> GetPolicyPaymentsAsync(Guid userId, Guid policyId, bool isStaff);

    // Staff generates an installment for a policy.
    Task<PremiumPaymentDto> CreatePremiumPaymentAsync(CreatePremiumPaymentDto payload);

    // Customer pays one of their installments.
    Task<PremiumPaymentDto> PayPremiumAsync(Guid userId, Guid paymentId, PayPremiumDto payload);

    // Staff correction.
    Task<PremiumPaymentDto> UpdatePremiumPaymentAsync(Guid paymentId, UpdatePremiumPaymentDto payload);
}
