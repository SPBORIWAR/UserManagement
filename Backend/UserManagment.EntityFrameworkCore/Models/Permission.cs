using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.EntityFrameworkCore.Models
{
    public class Permission : Entity<long>
    {
        public string Name { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }

}
