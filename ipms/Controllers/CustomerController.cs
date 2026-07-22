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
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomerController(ICustomerService service)
    {
        _service = service;
    }


    // Any signed-in user turns their account into a customer profile.
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer(CreateCustomerDto payload)
    {
        Guid userId = GetUserId();
        CustomerDto result = await _service.CreateCustomerAsync(userId, payload);
        return Ok(ApiResponse.Ok(result, "Customer profile created."));
    }


    // The signed-in customer views their own profile.
    [HttpGet("me")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetMyProfile()
    {
        CustomerDto result = await _service.GetMyProfileAsync(GetUserId());
        return Ok(ApiResponse.Ok(result, "Profile retrieved."));
    }


    [HttpPatch("me")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateMyProfile(UpdateCustomerDto payload)
    {
        CustomerDto result = await _service.UpdateMyProfileAsync(GetUserId(), payload);
        return Ok(ApiResponse.Ok(result, "Profile updated."));
    }


    // Staff can list and inspect customer records.
    [HttpGet]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<ApiResponse<CustomersDto>>> GetCustomers()
    {
        CustomersDto result = await _service.GetCustomersAsync();
        return Ok(ApiResponse.Ok(result, "Customers retrieved."));
    }


    [HttpGet("{customer_id}")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerById(Guid customer_id)
    {
        CustomerDto result = await _service.GetCustomerByIdAsync(customer_id);
        return Ok(ApiResponse.Ok(result, "Customer retrieved."));
    }


    [HttpPatch("{customer_id}")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateCustomerById(
        Guid customer_id,
        UpdateCustomerDto payload)
    {
        CustomerDto result = await _service.UpdateCustomerAsync(customer_id, payload);
        return Ok(ApiResponse.Ok(result, "Customer updated."));
    }


    [HttpDelete("{customer_id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteCustomerById(Guid customer_id)
    {
        await _service.DeleteCustomerAsync(customer_id);
        return Ok(ApiResponse.Ok(string.Empty, "Customer deleted."));
    }


    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
