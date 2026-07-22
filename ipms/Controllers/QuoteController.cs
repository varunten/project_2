using System.Security.Claims;
using IPMS.BAL.IService;
using IPMS.DTO;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuoteController : ControllerBase
{
    private readonly IQuoteService _service;

    public QuoteController(IQuoteService service)
    {
        _service = service;
    }


    // ---- Customer actions ----

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ApiResponse<QuoteDto>>> CreateQuote(CreateQuoteDto payload)
    {
        QuoteDto result = await _service.CreateQuoteAsync(GetUserId(), payload);
        return Ok(ApiResponse.Ok(result, "Quote requested."));
    }


    [HttpGet]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ApiResponse<QuotesDto>>> GetQuotes()
    {
        QuotesDto result = await _service.GetQuotesAsync(GetUserId());
        return Ok(ApiResponse.Ok(result, "Quotes retrieved."));
    }


    [HttpGet("{quote_id}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ApiResponse<QuoteDto>>> GetQuoteById(Guid quote_id)
    {
        QuoteDto result = await _service.GetQuoteByIdAsync(GetUserId(), quote_id);
        return Ok(ApiResponse.Ok(result, "Quote retrieved."));
    }


    [HttpPatch("{quote_id}/accept")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ApiResponse<QuoteDto>>> AcceptQuote(Guid quote_id)
    {
        QuoteDto result = await _service.AcceptQuoteAsync(GetUserId(), quote_id);
        return Ok(ApiResponse.Ok(result, "Quote accepted."));
    }


    [HttpPatch("{quote_id}/cancel")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ApiResponse<QuoteDto>>> CancelQuote(Guid quote_id)
    {
        QuoteDto result = await _service.CancelQuoteAsync(GetUserId(), quote_id);
        return Ok(ApiResponse.Ok(result, "Quote cancelled."));
    }


    // ---- Underwriter actions ----

    // The underwriter's review queue. Must come before the "{quote_id}" route
    // so the literal "pending" segment wins.
    [HttpGet("pending")]
    [Authorize(Roles = Roles.Underwriter)]
    public async Task<ActionResult<ApiResponse<QuotesDto>>> GetPendingQuotes()
    {
        QuotesDto result = await _service.GetPendingQuotesAsync();
        return Ok(ApiResponse.Ok(result, "Pending quotes retrieved."));
    }


    // Full details of any quote, so an underwriter can inspect before deciding.
    [HttpGet("review/{quote_id}")]
    [Authorize(Roles = Roles.Underwriter)]
    public async Task<ActionResult<ApiResponse<QuoteDto>>> GetQuoteForReview(Guid quote_id)
    {
        QuoteDto result = await _service.GetQuoteForReviewAsync(quote_id);
        return Ok(ApiResponse.Ok(result, "Quote retrieved."));
    }


    [HttpPatch("{quote_id}/approve")]
    [Authorize(Roles = Roles.Underwriter)]
    public async Task<ActionResult<ApiResponse<PolicyDto>>> ApproveQuote(Guid quote_id)
    {
        PolicyDto result = await _service.ApproveQuoteAsync(GetUserId(), quote_id);
        return Ok(ApiResponse.Ok(result, "Quote approved and policy issued."));
    }


    [HttpPatch("{quote_id}/reject")]
    [Authorize(Roles = Roles.Underwriter)]
    public async Task<ActionResult<ApiResponse<QuoteDto>>> RejectQuote(Guid quote_id)
    {
        QuoteDto result = await _service.RejectQuoteAsync(GetUserId(), quote_id);
        return Ok(ApiResponse.Ok(result, "Quote rejected."));
    }


    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
