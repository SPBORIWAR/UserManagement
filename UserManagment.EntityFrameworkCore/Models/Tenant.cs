using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.EntityFrameworkCore.Models
{
    public class Tenant : Entity<int>
    {
        public string Name { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }

}
