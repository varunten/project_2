using IPMS.DTO.Dtos;

namespace IPMS.BAL.IService;


public interface IQuoteService
{
    // Customer actions (scoped to the signed-in user's customer profile).
    Task<QuoteDto> CreateQuoteAsync(Guid userId, CreateQuoteDto payload);

    Task<QuotesDto> GetQuotesAsync(Guid userId);

    Task<QuoteDto> GetQuoteByIdAsync(Guid userId, Guid quoteId);

    Task<QuoteDto> AcceptQuoteAsync(Guid userId, Guid quoteId);

    // The customer withdraws their own quote before it becomes a policy.
    Task<QuoteDto> CancelQuoteAsync(Guid userId, Guid quoteId);

    // Underwriter actions.
    // Any quote, for staff review (not scoped to one customer).
    Task<QuoteDto> GetQuoteForReviewAsync(Guid quoteId);

    // Quotes accepted by a customer and waiting for a decision.
    Task<QuotesDto> GetPendingQuotesAsync();

    Task<PolicyDto> ApproveQuoteAsync(Guid underwriterId, Guid quoteId);

    Task<QuoteDto> RejectQuoteAsync(Guid underwriterId, Guid quoteId);
}
