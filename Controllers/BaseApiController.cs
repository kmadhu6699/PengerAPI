using Microsoft.AspNetCore.Mvc;

namespace PengerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        // Common controller functionality can go here
    }
}