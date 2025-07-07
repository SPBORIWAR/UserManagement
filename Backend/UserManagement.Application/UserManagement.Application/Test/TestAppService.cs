using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagement.BusinessLogic.SessionManagment;

namespace UserManagement.Application.Test
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TestAppService : ControllerBase
    {
        private readonly ISessionService _session;

        public TestAppService(ISessionService session)
        {
            _session = session;
        }

        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            var result = new
            {
                _session.UserId,
                _session.UserName,
                _session.TenantId,
                _session.Roles
            };

            return Ok(result);
        }

    }

}
