using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.Controllers;


// Small diagnostic endpoint: echoes the request headers of the caller.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataController : ControllerBase
{
    [HttpGet("test")]
    public ActionResult<string> GetData()
    {
        foreach (var header in Request.Headers)
        {
            Console.WriteLine($"{header.Key} -> {header.Value}");
        }

        return Ok("ok");
    }
}
