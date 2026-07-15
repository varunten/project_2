using IPMS.DTO.Entities;

namespace IPMS.DAL.IRepository;


public interface IQuoteRepository
{
    Task<List<Quote>> GetByCustomerIdAsync(Guid customerId);

    // Belonging to a specific customer (ownership check).
    Task<Quote?> GetByIdForCustomerAsync(Guid quoteId, Guid customerId);

    // Any quote (used by underwriters who review across customers).
    Task<Quote?> GetByIdAsync(Guid quoteId);

    // The underwriter's work queue: quotes the customer has accepted and that
    // are now waiting for a decision. Oldest first.
    Task<List<Quote>> GetPendingForReviewAsync();

    Task AddAsync(Quote quote);

    Task SaveChangesAsync();
}
