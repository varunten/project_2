using Microsoft.AspNetCore.Mvc;

namespace IPMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController: ControllerBase
{
    [HttpGet]
    public ActionResult<string> GetHealthStatus()
    {
        return Ok(new {
            status = "ok",
            current_time = DateTimeOffset.UtcNow
        });
    }
}