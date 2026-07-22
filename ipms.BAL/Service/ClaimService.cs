using IPMS.BAL.IService;
using IPMS.DAL.IRepository;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;
using IPMS.DTO.Enum;
using IPMS.DTO.Exceptions;

namespace IPMS.BAL.Service;


public class ClaimService : IClaimService
{
    private readonly IClaimRepository _repository;
    private readonly IPolicyRepository _policyRepository;
    private readonly ICustomerRepository _customerRepository;

    public ClaimService(
        IClaimRepository repository,
        IPolicyRepository policyRepository,
        ICustomerRepository customerRepository)
    {
        _repository = repository;
        _policyRepository = policyRepository;
        _customerRepository = customerRepository;
    }


    public async Task<ClaimDto> CreateClaimAsync(Guid userId, CreateClaimDto payload)
    {
        Customer customer = await _customerRepository.GetActiveByUserIdAsync(userId)
            ?? throw new BadRequestException("You must create a customer profile first.");

        Policy policy = await _policyRepository.GetByIdAsync(payload.PolicyId)
            ?? throw new NotFoundException("Policy not found.");

        if (policy.CustomerId != customer.Id)
            throw new ForbiddenException("You can only file claims on your own policies.");

        if (policy.Status != PolicyStatus.Active)
            throw new ConflictException("Claims can only be filed on active policies.");

        if (payload.ClaimAmount <= 0)
            throw new ValidationException("Claim amount must be greater than zero.");

        Claim claim = new()
        {
            ClaimNumber = $"CLM-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            PolicyId = payload.PolicyId,
            UnderWriterId = null,
            IncidentDate = payload.IncidentDate,
            ClaimAmount = payload.ClaimAmount,
            Reason = payload.Reason,
            Status = ClaimStatus.Submitted
        };

        await _repository.AddAsync(claim);
        await _repository.SaveChangesAsync();

        return MapToDto(claim);
    }


    public async Task<ClaimsDto> GetMyClaimsAsync(Guid userId)
    {
        Customer customer = await _customerRepository.GetActiveByUserIdAsync(userId)
            ?? throw new BadRequestException("You must create a customer profile first.");

        List<Policy> policies = await _policyRepository.GetByCustomerIdAsync(customer.Id);

        List<Claim> claims = await _repository.GetByPolicyIdsAsync(
            policies.Select(p => p.Id).ToList());

        List<ClaimDto> dtos = claims.Select(MapToDto).ToList();

        return new ClaimsDto
        {
            Total = (ulong)dtos.Count,
            Claims = dtos
        };
    }


    public async Task<ClaimDto> GetMyClaimByIdAsync(Guid userId, Guid claimId)
    {
        Customer customer = await _customerRepository.GetActiveByUserIdAsync(userId)
            ?? throw new BadRequestException("You must create a customer profile first.");

        Claim claim = await _repository.GetByIdAsync(claimId)
            ?? throw new NotFoundException("Claim not found.");

        Policy? policy = await _policyRepository.GetByIdAsync(claim.PolicyId);

        // Hide the existence of claims that aren't the customer's own.
        if (policy is null || policy.CustomerId != customer.Id)
            throw new NotFoundException("Claim not found.");

        return MapToDto(claim);
    }


    public async Task<ClaimsDto> GetClaimsAsync()
    {
        List<Claim> claims = await _repository.GetAllAsync();

        List<ClaimDto> dtos = claims.Select(MapToDto).ToList();

        return new ClaimsDto
        {
            Total = (ulong)dtos.Count,
            Claims = dtos
        };
    }


    public async Task<ClaimDto> GetClaimByIdAsync(Guid claimId)
    {
        Claim claim = await _repository.GetByIdAsync(claimId)
            ?? throw new NotFoundException("Claim not found.");

        return MapToDto(claim);
    }


    public async Task<ClaimDto> UpdateClaimAsync(Guid underwriterId, Guid claimId, UpdateClaimDto payload)
    {
        Claim claim = await _repository.GetByIdAsync(claimId)
            ?? throw new NotFoundException("Claim not found.");

        // The reviewing underwriter takes ownership of the claim.
        claim.UnderWriterId = payload.UnderWriterId ?? underwriterId;

        if (payload.ClaimAmount.HasValue) claim.ClaimAmount = payload.ClaimAmount.Value;
        if (payload.Notes is not null) claim.Notes = payload.Notes;

        if (payload.Status.HasValue)
        {
            claim.Status = payload.Status.Value;

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (claim.Status == ClaimStatus.Approved)
            {
                claim.ApprovedAmount = claim.ClaimAmount;
                claim.SettledDate = today;
            }
            else if (claim.Status is ClaimStatus.Rejected or ClaimStatus.Closed)
            {
                claim.SettledDate = today;
            }
        }

        await _repository.SaveChangesAsync();

        return MapToDto(claim);
    }


    public async Task<ClaimDocumentDto> UploadDocumentAsync(Guid userId, Guid claimId, UploadClaimDocumentDto payload)
    {
        Claim _ = await _repository.GetByIdAsync(claimId)
            ?? throw new NotFoundException("Claim not found.");

        ClaimDocument document = new()
        {
            ClaimId = claimId,
            FileName = payload.FileName,
            FileType = payload.FileType,
            FileURL = payload.FileURL,
            UploadedAt = DateTimeOffset.UtcNow,
            UploadedBy = userId
        };

        await _repository.AddDocumentAsync(document);
        await _repository.SaveChangesAsync();

        return MapDocumentToDto(document);
    }


    public async Task<List<ClaimDocumentDto>> GetClaimDocumentsAsync(Guid claimId)
    {
        Claim _ = await _repository.GetByIdAsync(claimId)
            ?? throw new NotFoundException("Claim not found.");

        List<ClaimDocument> documents = await _repository.GetDocumentsByClaimIdAsync(claimId);

        return documents.Select(MapDocumentToDto).ToList();
    }


    private static ClaimDto MapToDto(Claim c)
    {
        return new ClaimDto
        {
            Id = c.Id,
            ClaimNumber = c.ClaimNumber,
            PolicyId = c.PolicyId,
            UnderWriterId = c.UnderWriterId,
            IncidentDate = c.IncidentDate,
            SettledDate = c.SettledDate,
            ClaimAmount = c.ClaimAmount,
            ApprovedAmount = c.ApprovedAmount,
            Reason = c.Reason,
            Notes = c.Notes,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }


    private static ClaimDocumentDto MapDocumentToDto(ClaimDocument d)
    {
        return new ClaimDocumentDto
        {
            Id = d.Id,
            ClaimId = d.ClaimId,
            FileName = d.FileName,
            FileType = d.FileType,
            FileURL = d.FileURL,
            UploadedAt = d.UploadedAt,
            UploadedBy = d.UploadedBy
        };
    }
}
