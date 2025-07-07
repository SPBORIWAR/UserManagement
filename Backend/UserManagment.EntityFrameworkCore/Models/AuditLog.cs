using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.EntityFrameworkCore.Models
{
    public class AuditLog : Entity<long>
    {
        public string Action { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }

        public DateTime Timestamp { get; set; }

        public AuditLog()
        {
            Timestamp = DateTime.UtcNow;
        }
    }

}
