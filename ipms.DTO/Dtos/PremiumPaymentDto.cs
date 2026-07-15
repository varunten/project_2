
using System.ComponentModel.DataAnnotations;
using IPMS.DTO.Enum;

namespace IPMS.DTO.Dtos;


public class PremiumPaymentDto
{
    public required Guid Id { get; set; }
    public required Guid PolicyId { get; set; }
    public required string InstallmentNumber { get; set; }
    public required decimal PremiumAmount { get; set; }
    public required decimal PenaltyAmount { get; set; }
    public required decimal TotalPaid { get; set; }
    public required PremiumFrequency Frequency {get; set;}
    public required DateOnly DueDate { get; set; }
    public DateOnly? PaidDate { get; set; }
    public required PremiumPaymentStatus PaymentStatus { get; set; }
    public PremiumPaymentMethod? PaymentMethod { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }
}


public class PremiumPaymentsDto
{
    public required ulong Total { get; set; }
    public required List<PremiumPaymentDto> Payments { get; set; }
}


public class CreatePremiumPaymentDto
{
    public required Guid PolicyId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Installment number is required.")]
    public required string InstallmentNumber { get; set; }

    public required PremiumFrequency Frequency {get; set;}

    [Range(1, 100000000, ErrorMessage = "Premium amount must be greater than zero.")]
    public required decimal PremiumAmount { get; set; }

    [Range(0, 100000000, ErrorMessage = "Penalty amount cannot be negative.")]
    public required decimal PenaltyAmount { get; set; }

    public required DateOnly DueDate { get; set; }
}


public class PayPremiumDto
{
    [Range(1, 100000000, ErrorMessage = "Amount paid must be greater than zero.")]
    public required decimal AmountPaid { get; set; }

    public required PremiumPaymentMethod PaymentMethod { get; set; }
}


public class UpdatePremiumPaymentDto
{
    public PremiumPaymentStatus? PaymentStatus { get; set; }
    public PremiumPaymentMethod? PaymentMethod { get; set; }
    public decimal? PenaltyAmount { get; set; }
}