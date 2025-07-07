using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.EntityFrameworkCore.Models
{
    public class Notification : Entity<long>
    {
        public string Message { get; set; }
        public long TargetUserId { get; set; }
        public User TargetUser { get; set; }

        public bool IsRead { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }

}
