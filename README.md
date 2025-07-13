using Microsoft.AspNetCore.Mvc;

namespace PengerApi.Controllers
{
    public class HealthCheckController : BaseApiController
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}