using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.EntityFrameworkCore.Models
{
    public class Chat : Entity<long>
    {
        public long FromUserId { get; set; }
        public User FromUser { get; set; }

        public long ToUserId { get; set; }
        public User ToUser { get; set; }

        public string Message { get; set; }
        public DateTime SentAt { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }

        public Chat()
        {
            SentAt = DateTime.UtcNow;
        }
    }

}
