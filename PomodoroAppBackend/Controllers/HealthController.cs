using Microsoft.AspNetCore.Mvc;

namespace PomodoroAppBackend.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Healthy");
}
