using System.Security.Claims;
using IPMS.BAL.IService;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ISessionService _service;

    public SessionController(ISessionService service)
    {
        _service = service;
    }


    [HttpPost]
    public async Task<ActionResult<ApiResponse<SessionsDto>>> GetCurrentSessions(RefreshTokenDto payload)
    {
        SessionsDto result = await _service.GetSessionsAsync(GetUserId(), payload.Token);
        return Ok(ApiResponse.Ok(result, "Sessions retrieved."));
    }


    [HttpDelete]
    public async Task<ActionResult<ApiResponse<string>>> RevokeAllSessions()
    {
        await _service.RevokeAllSessionsAsync(GetUserId());
        return Ok(ApiResponse.Ok(string.Empty, "All sessions revoked."));
    }


    [HttpDelete("{family_id}")]
    public async Task<ActionResult<ApiResponse<string>>> RevokeSessionByFamilyId(Guid family_id)
    {
        await _service.RevokeSessionAsync(GetUserId(), family_id);
        return Ok(ApiResponse.Ok(string.Empty, "Session revoked."));
    }


    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
