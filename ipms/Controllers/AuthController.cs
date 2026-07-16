using IPMS.BAL.IService;
using IPMS.DTO;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }


    [HttpPost("signup")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register(AuthSignupDto payload)
    {
        UserDto result = await _service.RegisterAsync(payload);
        return Ok(ApiResponse.Ok(result, "User registered successfully."));
    }


    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<TokenDto>>> Login(AuthLoginDto payload)
    {
        TokenDto result = await _service.LoginAsync(payload);
        return Ok(ApiResponse.Ok(result, "Login successful."));
    }


    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<TokenDto>>> Refresh(RefreshTokenDto payload)
    {
        TokenDto result = await _service.RefreshAsync(payload);
        return Ok(ApiResponse.Ok(result, "Token refreshed."));
    }


    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<string>>> Logout(RefreshTokenDto payload)
    {
        await _service.LogoutAsync(payload);
        return Ok(ApiResponse.Ok(string.Empty, "Logged out successfully."));
    }


    // Admin creates staff accounts (Admin / InsuranceAgent / Underwriter).
    [HttpPost("staff")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateStaff(CreateStaffDto payload)
    {
        UserDto result = await _service.CreateStaffAsync(payload);
        return Ok(ApiResponse.Ok(result, "Staff account created."));
    }


    // Admin lists every user and the roles they hold.
    [HttpGet("users")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<UsersDto>>> GetUsers()
    {
        UsersDto result = await _service.GetAllUsersAsync();
        return Ok(ApiResponse.Ok(result, "Users retrieved."));
    }


    // Admin promotes an existing user by assigning them another role.
    [HttpPost("users/{user_id}/roles")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<List<string>>>> AssignRole(
        Guid user_id,
        AssignRoleDto payload)
    {
        List<string> roles = await _service.AssignRoleAsync(user_id, payload.Role);
        return Ok(ApiResponse.Ok(roles, "Role assigned. The user must log in again to use it."));
    }
}
