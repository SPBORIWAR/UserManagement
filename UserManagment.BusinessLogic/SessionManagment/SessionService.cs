using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.BusinessLogic.SessionManagment
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long UserId => long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public string UserName => _httpContextAccessor.HttpContext.User.Identity?.Name;

        public int TenantId => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue("TenantId"));
        public int RoleId => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue("RoleId"));

        public List<string> Roles => _httpContextAccessor.HttpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }


}
