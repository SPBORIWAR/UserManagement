using Microsoft.EntityFrameworkCore;
using UserManagement.BusinessLogic.Dtos;
using UserManagement.BusinessLogic.SessionManagment;
using UserManagement.EntityFrameworkCore.Models;
using UserManagement.EntityFrameworkCore.Repository;

namespace UserManagement.BusinessLogic.RoleManagement
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly ISessionService _session;


        public RoleService(
            IRepository<UserRole> userRoleRepository,
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        ISessionService session)
        {
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
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

        public async Task<List<RoleDto>> GetRolesByUserIdAsync(long userId)
        {
            var tenantId = _session.TenantId;

            // Get all role mappings for this user within the tenant
            var userRoles = await _userRoleRepository.GetAllIncluding(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
                .ToListAsync();

            if (!userRoles.Any())
                throw new InvalidOperationException("No roles assigned to this user in your tenant.");

            var roleDtos = userRoles.Select(ur => new RoleDto
            {
                Id = ur.Role.Id,
                Name = ur.Role.Name,
                TenantId = ur.Role.TenantId
            }).ToList();

            return roleDtos;
        }

        public async Task<RoleDto> GetPrimaryRoleByUserIdAsync(long userId)
        {
            var tenantId = _session.TenantId;

            var userRole = await _userRoleRepository.GetAllIncluding(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
                .OrderBy(ur => ur.CreatedDate) // Assuming earlier is primary
                .FirstOrDefaultAsync();

            if (userRole == null)
                throw new InvalidOperationException("No roles assigned to this user in your tenant.");

            return new RoleDto
            {
                Id = userRole.Role.Id,
                Name = userRole.Role.Name,
                TenantId = userRole.Role.TenantId
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

        public async Task AssignRoleToUserAsync(long userId, int roleId)
        {
            CheckRolePermission();

            var tenantId = _session.TenantId;

            var user = await _userRepository.GetByIdAsync(userId);
            var role = await _roleRepository.GetByIdAsync(roleId);

            if (user == null || role == null || user.TenantId != tenantId || role.TenantId != tenantId)
                throw new UnauthorizedAccessException("User or Role not found in your tenant.");

            // Check if already assigned
            var exists = await _userRoleRepository.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.TenantId == tenantId);
            if (exists)
                throw new InvalidOperationException("Role already assigned to user.");

            await _userRoleRepository.InsertAsync(new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                TenantId = tenantId,
                CreatedBy = _session.UserId,
                CreatedDate = DateTime.UtcNow
            });

            await _userRoleRepository.SaveChangesAsync();
        }

        public async Task RemoveRoleFromUserAsync(long userId, int roleId)
        {
            CheckRolePermission();

            var tenantId = _session.TenantId;

            var userRole = await _userRoleRepository.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.TenantId == tenantId);
            if (userRole == null)
                throw new InvalidOperationException("Role not assigned to user.");

            await _userRoleRepository.DeleteAsync(userRole);
            await _userRoleRepository.SaveChangesAsync();
        }
    }
}