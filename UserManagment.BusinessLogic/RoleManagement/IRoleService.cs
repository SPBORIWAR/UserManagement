using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.BusinessLogic.Dtos;

namespace UserManagement.BusinessLogic.RoleManagement
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(long id);
        Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
        Task UpdateRoleAsync(RoleDto dto);
        Task DeleteRoleAsync(long id);
        Task AssignRoleToUserAsync(long userId, int roleId);
        Task RemoveRoleFromUserAsync(long userId, int roleId);
        Task<List<RoleDto>> GetRolesByUserIdAsync(long userId);
        Task<RoleDto> GetPrimaryRoleByUserIdAsync(long userId);

    }
}
