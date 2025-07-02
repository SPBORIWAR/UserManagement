using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Application.HealthCheck
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckAppService : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
