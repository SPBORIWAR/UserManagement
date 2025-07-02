using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.BusinessLogic.Dtos;

namespace UserManagement.BusinessLogic.PermissionManagement
{
    public interface IPermissionService
    {
        Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto dto);
        Task<List<PermissionDto>> GetAllPermissionsAsync();

        Task AssignPermissionToRoleAsync(int roleId, long permissionId);
        Task RemovePermissionFromRoleAsync(int roleId, long permissionId);
        Task<List<string>> GetPermissionsByRoleAsync(int roleId);
        Task<List<string>> GetGrantedPermissionsForLoggedInUserAsync();

    }


}
