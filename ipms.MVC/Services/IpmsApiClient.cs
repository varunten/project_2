using System.Net.Http.Json;
using System.Text.Json;
using IPMS.DTO.Dtos;

namespace ipms.MVC.Services;


// The only place that talks to the IPMS API. Every method sends a request,
// unwraps the ApiResponse<T> envelope, and returns the plain DTO.
public class IpmsApiClient
{
    private readonly HttpClient _http;

    // "Web" defaults = camelCase property matching, which is what the API sends.
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public IpmsApiClient(HttpClient http)
    {
        _http = http;
    }


    // ---- Auth ----

    public Task<UserDto> SignupAsync(AuthSignupDto payload) =>
        SendAsync<UserDto>(HttpMethod.Post, "api/auth/signup", payload);

    public Task<TokenDto> LoginAsync(AuthLoginDto payload) =>
        SendAsync<TokenDto>(HttpMethod.Post, "api/auth/login", payload);


    // ---- Admin ----

    public Task<UsersDto> GetUsersAsync() =>
        SendAsync<UsersDto>(HttpMethod.Get, "api/auth/users");

    public Task<UserDto> CreateStaffAsync(CreateStaffDto payload) =>
        SendAsync<UserDto>(HttpMethod.Post, "api/auth/staff", payload);

    public Task<List<string>> AssignRoleAsync(Guid userId, AssignRoleDto payload) =>
        SendAsync<List<string>>(HttpMethod.Post, $"api/auth/users/{userId}/roles", payload);

    public Task<ProductDto> CreateProductAsync(CreateProductDto payload) =>
        SendAsync<ProductDto>(HttpMethod.Post, "api/product", payload);


    // ---- Customer profile ----

    public Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto payload) =>
        SendAsync<CustomerDto>(HttpMethod.Post, "api/customer", payload);


    // ---- Products ----

    public Task<ProductsDto> GetProductsAsync() =>
        SendAsync<ProductsDto>(HttpMethod.Get, "api/product");

    public Task<ProductDto> GetProductAsync(Guid productId) =>
        SendAsync<ProductDto>(HttpMethod.Get, $"api/product/{productId}");


    // ---- Quotes ----

    public Task<QuotesDto> GetQuotesAsync() =>
        SendAsync<QuotesDto>(HttpMethod.Get, "api/quote");

    public Task<QuoteDto> CreateQuoteAsync(CreateQuoteDto payload) =>
        SendAsync<QuoteDto>(HttpMethod.Post, "api/quote", payload);

    public Task<QuoteDto> AcceptQuoteAsync(Guid quoteId) =>
        SendAsync<QuoteDto>(HttpMethod.Patch, $"api/quote/{quoteId}/accept");


    // ---- Underwriter ----

    public Task<QuotesDto> GetPendingQuotesAsync() =>
        SendAsync<QuotesDto>(HttpMethod.Get, "api/quote/pending");

    // Approving a quote issues the policy, so the API hands back the policy.
    public Task<PolicyDto> ApproveQuoteAsync(Guid quoteId) =>
        SendAsync<PolicyDto>(HttpMethod.Patch, $"api/quote/{quoteId}/approve");

    public Task<QuoteDto> RejectQuoteAsync(Guid quoteId) =>
        SendAsync<QuoteDto>(HttpMethod.Patch, $"api/quote/{quoteId}/reject");


    // ---- Policies ----

    public Task<PoliciesDto> GetPoliciesAsync() =>
        SendAsync<PoliciesDto>(HttpMethod.Get, "api/policy");

    public Task<PolicyDto> GetPolicyAsync(Guid policyId) =>
        SendAsync<PolicyDto>(HttpMethod.Get, $"api/policy/{policyId}");


    // ---- Premium payments ----

    public Task<PremiumPaymentsDto> GetPolicyPaymentsAsync(Guid policyId) =>
        SendAsync<PremiumPaymentsDto>(HttpMethod.Get, $"api/premiumpayment/policy/{policyId}");

    public Task<PremiumPaymentDto> PayPremiumAsync(Guid paymentId, PayPremiumDto payload) =>
        SendAsync<PremiumPaymentDto>(HttpMethod.Post, $"api/premiumpayment/{paymentId}/pay", payload);


    // ---- Claims ----

    public Task<ClaimsDto> GetMyClaimsAsync() =>
        SendAsync<ClaimsDto>(HttpMethod.Get, "api/claim/my");

    public Task<ClaimDto> CreateClaimAsync(CreateClaimDto payload) =>
        SendAsync<ClaimDto>(HttpMethod.Post, "api/claim", payload);

    // Staff / underwriter
    public Task<ClaimsDto> GetAllClaimsAsync() =>
        SendAsync<ClaimsDto>(HttpMethod.Get, "api/claim");

    public Task<ClaimDto> UpdateClaimAsync(Guid claimId, UpdateClaimDto payload) =>
        SendAsync<ClaimDto>(HttpMethod.Patch, $"api/claim/{claimId}", payload);


    // ---- the one method that does the actual work ----

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string url,
        object? body = null)
    {
        using HttpRequestMessage request = new(method, url);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using HttpResponseMessage response = await _http.SendAsync(request);

        string json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ErrorResponse? error = TryRead<ErrorResponse>(json);

            throw new ApiException(
                (int)response.StatusCode,
                error?.Message ?? $"The API returned {(int)response.StatusCode}.",
                error?.Errors);
        }

        ApiResponse<T>? success = TryRead<ApiResponse<T>>(json);

        if (success is null || success.Data is null)
        {
            throw new ApiException(500, "The API returned an unexpected response.");
        }

        return success.Data;
    }


    private static TValue? TryRead<TValue>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<TValue>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
