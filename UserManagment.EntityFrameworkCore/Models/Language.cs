using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.EntityFrameworkCore.Models
{
    public class Language : Entity<long>
    {
        public string Name { get; set; }
        public string CultureCode { get; set; }
        public string? FlagIcon { get; set; }
    }

}
