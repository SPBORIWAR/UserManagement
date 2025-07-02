using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.BusinessLogic.SessionManagment
{
    public interface ISessionService
    {
        long UserId { get; }
        string UserName { get; }
        int TenantId { get; }
        int RoleId { get; }
        List<string> Roles { get; }
    }
}
