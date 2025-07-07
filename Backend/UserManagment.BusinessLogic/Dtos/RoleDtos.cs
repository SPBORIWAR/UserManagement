using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.BusinessLogic.Dtos
{
    public class RoleDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int? TenantId { get; set; }
    }

    public class CreateRoleDto
    {
        public string Name { get; set; }
        public int? TenantId { get; set; }
    }

    public class UpdateRoleDto
    {
        public string Name { get; set; }
        public int? TenantId { get; set; }
    }

    public class AssignRoleDto
    {
        public long UserId { get; set; }
        public int RoleId { get; set; }
    }



}
