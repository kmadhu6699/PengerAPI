using Microsoft.AspNetCore.Mvc;

namespace PengerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        // Common controller functionality can go here
    }
}