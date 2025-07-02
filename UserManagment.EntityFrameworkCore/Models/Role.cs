using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.EntityFrameworkCore.Models
{
    public class Role : Entity<int>
    {
        public string Name { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

}
