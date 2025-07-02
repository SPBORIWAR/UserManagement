using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.EntityFrameworkCore.Models
{
    public abstract class Entity<T>
    {
        public T Id { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        protected Entity()
        {
            CreatedDate = DateTime.UtcNow;
        }
    }
}
