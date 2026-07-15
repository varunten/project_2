using IPMS.DAL.Data;
using IPMS.DAL.IRepository;
using IPMS.DTO.Entities;
using IPMS.DTO.Enum;
using Microsoft.EntityFrameworkCore;

namespace IPMS.DAL.Repository;


public class QuoteRepository : IQuoteRepository
{
    private readonly AppDbContext _context;

    public QuoteRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<List<Quote>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Quotes
            .Where(q => q.CustomerId == customerId && q.DeletedAt == null)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }


    public async Task<Quote?> GetByIdForCustomerAsync(Guid quoteId, Guid customerId)
    {
        return await _context.Quotes
            .FirstOrDefaultAsync(q =>
                q.Id == quoteId &&
                q.CustomerId == customerId &&
                q.DeletedAt == null);
    }


    public async Task<Quote?> GetByIdAsync(Guid quoteId)
    {
        return await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.DeletedAt == null);
    }


    public async Task<List<Quote>> GetPendingForReviewAsync()
    {
        return await _context.Quotes
            .Where(q =>
                q.Status == QuoteStatus.AcceptedByCustomer &&
                q.DeletedAt == null)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();
    }


    public async Task AddAsync(Quote quote)
    {
        await _context.Quotes.AddAsync(quote);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
