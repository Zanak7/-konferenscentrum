using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KonferenscentrumVast.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        [Authorize] // kräver JWT
        public IActionResult Get() => Ok(new { pong = true, timeUtc = DateTime.UtcNow });
    }
}