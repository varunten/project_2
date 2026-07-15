using IPMS.DTO.Enum;

namespace IPMS.DTO.Entities;






public class PremiumPayment: BaseEntity
{
    public required Guid PolicyId {get; set;}
    public required string InstallmentNumber {get; set;}
    public required decimal PremiumAmount {get; set;}
    public required PremiumFrequency Frequency {get; set;}
    public required decimal PenaltyAmount {get; set;}
    public decimal TotalPaid {get; set;}
    public required DateOnly DueDate {get; set;}
    public DateOnly? PaidDate {get; set;}
    public required PremiumPaymentStatus PaymentStatus {get; set;}
    // Only known once the installment is actually paid.
    public PremiumPaymentMethod? PaymentMethod {get; set;}
}