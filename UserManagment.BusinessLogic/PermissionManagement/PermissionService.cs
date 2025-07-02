using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.BusinessLogic.Dtos;
using UserManagement.BusinessLogic.SessionManagment;
using UserManagement.EntityFrameworkCore.Models;
using UserManagement.EntityFrameworkCore.Repository;

namespace UserManagement.BusinessLogic.PermissionManagement
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        private readonly ISessionService _session;

        public PermissionService(
            IRepository<Permission> permissionRepo,
            IRepository<RolePermission> rolePermissionRepo,
            ISessionService session)
        {
            _permissionRepo = permissionRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _session = session;
        }

        // 📌 Create a permission (for this tenant)
        public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto dto)
        {
            var permission = new Permission
            {
                Name = dto.Name,
                TenantId = _session.TenantId,
                CreatedBy = _session.UserId,
                CreatedDate = DateTime.UtcNow
            };

            await _permissionRepo.InsertAsync(permission);
            await _permissionRepo.SaveChangesAsync();

            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                TenantId = permission.TenantId
            };
        }

        // 📌 Get all permissions in the tenant
        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _permissionRepo.GetListAsync(p => p.TenantId == _session.TenantId);
            return permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                TenantId = p.TenantId
            }).ToList();
        }

        // 📌 Assign permission to a role in the current tenant
        public async Task AssignPermissionToRoleAsync(int roleId, long permissionId)
        {
            var entity = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                TenantId = _session.TenantId,
                CreatedBy = _session.UserId,
                CreatedDate = DateTime.UtcNow
            };

            await _rolePermissionRepo.InsertAsync(entity);
            await _rolePermissionRepo.SaveChangesAsync();
        }

        // 📌 Remove a permission assignment from a role
        public async Task RemovePermissionFromRoleAsync(int roleId, long permissionId)
        {
            var entity = await _rolePermissionRepo.FirstOrDefaultAsync(rp =>
                rp.RoleId == roleId && rp.PermissionId == permissionId && rp.TenantId == _session.TenantId);

            if (entity != null)
            {
                await _rolePermissionRepo.DeleteAsync(entity);
                await _rolePermissionRepo.SaveChangesAsync();
            }
        }

        // 📌 Get permissions by role in current tenant
        public async Task<List<string>> GetPermissionsByRoleAsync(int roleId)
        {
            var rolePermissions = await _rolePermissionRepo.GetListAsync(rp =>
                rp.RoleId == roleId && rp.TenantId == _session.TenantId);

            return rolePermissions.Select(rp => rp.Permission.Name).Distinct().ToList();
        }

        // 📌 Get granted permissions for logged-in user
        public async Task<List<string>> GetGrantedPermissionsForLoggedInUserAsync()
        {
            var userRoleId = _session.RoleId;
            var tenantId = _session.TenantId;

            var rolePermissions = await _rolePermissionRepo
                .GetAllIncluding(rp => rp.Permission)
                .Where(rp => rp.RoleId == userRoleId && rp.TenantId == tenantId)
                .ToListAsync();

            var permissions = rolePermissions
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList();

            return permissions;
        }


    }


}
