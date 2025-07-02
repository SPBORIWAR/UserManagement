using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.BusinessLogic.Dtos
{
    public class RegisterUserDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TenantId { get; set; }
        public int RoleId { get; set; }
    }


    public class LoginDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int TenantId { get; set; }
    }


    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }



}
