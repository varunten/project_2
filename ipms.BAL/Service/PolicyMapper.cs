using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;

namespace IPMS.BAL.Service;


// Shared Policy -> PolicyDto mapping. Used by both PolicyService and
// QuoteService (approving a quote creates and returns a policy).
public static class PolicyMapper
{
    public static PolicyDto ToDto(Policy p, string productName)
    {
        return new PolicyDto
        {
            Id = p.Id,
            PolicyNumber = p.PolicyNumber,
            QuoteId = p.QuoteId,
            ProductId = p.ProductId,
            ProductName = productName,
            InsuranceAgentId = p.InsuranceAgentId,
            UnderWriterId = p.UnderWriterId,
            CustomerId = p.CustomerId,
            CoverageAmount = p.CoverageAmount,
            PremiumAmount = p.PremiumAmount,
            QuoteDate = p.QuoteDate,
            IssueDate = p.IssueDate,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}
