using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.BusinessLogic.Dtos
{
    public class CreateTenantDto
    {
        public string Name { get; set; }
    }

    public class UpdateTenantDto
    {
        public string Name { get; set; }
    }

    public class TenantDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }


}
