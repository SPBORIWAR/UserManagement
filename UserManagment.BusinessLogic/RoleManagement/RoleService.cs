using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.BusinessLogic.Dtos;
using UserManagement.BusinessLogic.SessionManagment;
using UserManagement.EntityFrameworkCore.Models;
using UserManagement.EntityFrameworkCore.Repository;

namespace UserManagement.BusinessLogic.RoleManagement
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly ISessionService _session;

        public RoleService(IRepository<Role> roleRepository, ISessionService session)
        {
            _roleRepository = roleRepository;
            _session = session;
        }

        private void CheckRolePermission()
        {
            if (!_session.Roles.Contains("SuperAdmin") && !_session.Roles.Contains("Admin"))
                throw new UnauthorizedAccessException("Only SuperAdmin or TenantAdmin can manage roles.");
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var tenantId = _session.TenantId;

            var roles = await _roleRepository.GetAllAsync();
            var filteredRoles = roles.Where(r => r.TenantId == tenantId).ToList();

            return filteredRoles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                TenantId = r.TenantId
            }).ToList();
        }

        public async Task<RoleDto> GetRoleByIdAsync(long id)
        {
            var tenantId = _session.TenantId;            
            var role = await _roleRepository.FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
            if (role == null || role.TenantId != _session.TenantId)
                throw new UnauthorizedAccessException("Cannot modify roles outside your tenant.");

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                TenantId = role.TenantId
            };
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            CheckRolePermission();

            var tenantIdToUse = _session.TenantId != 0 ? _session.TenantId : dto.TenantId; 
            var sessionUserId = _session.UserId;
            var role = new Role
            {
                Name = dto.Name,
                TenantId = Convert.ToInt32(tenantIdToUse),
                CreatedBy = sessionUserId,
                CreatedDate = DateTime.UtcNow
            };

            await _roleRepository.InsertAsync(role);
            await _roleRepository.SaveChangesAsync();

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                TenantId = role.TenantId
            };
        }

        public async Task UpdateRoleAsync(RoleDto dto)
        {
            CheckRolePermission();
            var tenantIdToUse = _session.TenantId != 0 ? _session.TenantId : dto.TenantId;            
            var role = await _roleRepository.FirstOrDefaultAsync(r => r.Id == dto.Id && r.TenantId == tenantIdToUse); ;
            if (role == null || role.TenantId != _session.TenantId)
                throw new UnauthorizedAccessException("Cannot modify roles outside your tenant.");

            role.Name = dto.Name;
            await _roleRepository.UpdateAsync(role);  
            await _roleRepository.SaveChangesAsync();
        }

        public async Task DeleteRoleAsync(long id)
        {
            CheckRolePermission();

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null || role.TenantId != _session.TenantId)
                throw new UnauthorizedAccessException("Cannot delete roles outside your tenant.");

            await _roleRepository.DeleteAsync(role);
            await _roleRepository.SaveChangesAsync();
        }
    }


}
