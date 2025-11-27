using Microsoft.AspNetCore.Identity;
using UserManagement.BusinessLogic.Dtos;
using UserManagement.BusinessLogic.SessionManagment;
using UserManagement.EntityFrameworkCore.Models;
using UserManagement.EntityFrameworkCore.Repository;

namespace UserManagement.BusinessLogic.TenantManagement
{
    public class TenantService : ITenantService
    {
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ISessionService _sessionService;
        private readonly IRepository<RolePermission> _rolePermissionRepository;
        private readonly IRepository<Permission> _permissionRepository;

        public TenantService(
            IRepository<Tenant> tenantRepository,
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRole> userRoleRepository,
            IPasswordHasher<User> passwordHasher,
            ISessionService sessionService,
            IRepository<RolePermission> rolePermissionRepository,
            IRepository<Permission> permissionRepository
            )
        {
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _passwordHasher = passwordHasher;
            _sessionService = sessionService;
            _rolePermissionRepository = rolePermissionRepository;
            _permissionRepository = permissionRepository;
        }

        public async Task<TenantDto> CreateTenantAsync(CreateTenantDto dto)
        {
            using var transaction = await _tenantRepository.BeginTransactionAsync();
            var sessionUserId = _sessionService.UserId;

            try
            {
                var tenant = new Tenant
                {
                    Name = dto.Name,
                    CreatedBy = sessionUserId,
                    CreatedDate = DateTime.UtcNow
                };

                await _tenantRepository.InsertAsync(tenant);
                await _tenantRepository.SaveChangesAsync();

                // Create Admin and User roles
                var adminRole = new Role
                {
                    Name = "Admin",
                    TenantId = tenant.Id,
                    CreatedBy = sessionUserId,
                    CreatedDate = DateTime.UtcNow
                };
                var userRole = new Role
                {
                    Name = "User",
                    TenantId = tenant.Id,
                    CreatedBy = sessionUserId,
                    CreatedDate = DateTime.UtcNow
                };

                var adminRoleId = await _roleRepository.InsertAndGetIdAsync(adminRole);
                var userRoleId = await _roleRepository.InsertAndGetIdAsync(userRole);

                // Seed default permissions for the tenant
                var defaultPermissions = new List<string>
                {
                    "Pages",
                    "Pages.Dashboard",
                    "Pages.Administration",
                    "Pages.Administration.Tenant",
                    "Pages.Administration.Tenant.Create",
                    "Pages.Administration.Role",
                    "Pages.Administration.Role.Create",
                    "Pages.Administration.Permission",
                    "Pages.Administration.Permission.Create"
                };

                var permissionIds = new List<long>();

                foreach (var permissionName in defaultPermissions)
                {
                    var permission = new Permission
                    {
                        Name = permissionName,
                        TenantId = tenant.Id,
                        CreatedBy = sessionUserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    var permissionId = await _permissionRepository.InsertAndGetIdAsync(permission);
                    permissionIds.Add((long)permissionId);
                }
                await _permissionRepository.SaveChangesAsync();

                // Assign permissions to Admin role
                foreach (var permissionId in permissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = (int)adminRoleId,
                        PermissionId = permissionId,
                        TenantId = tenant.Id,
                        CreatedBy = sessionUserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _rolePermissionRepository.InsertAsync(rolePermission);
                }
                await _rolePermissionRepository.SaveChangesAsync();

                // Create Admin user WITHOUT RoleId
                var adminUser = new User
                {
                    UserName = $"{tenant.Name.ToLower()}Admin",
                    FirstName = "Admin",
                    LastName = tenant.Name,
                    TenantId = tenant.Id,
                    CreatedBy = sessionUserId,
                    CreatedDate = DateTime.UtcNow
                };

                adminUser.Password = _passwordHasher.HashPassword(adminUser, "Admin@123");
                var adminUserId = await _userRepository.InsertAndGetIdAsync(adminUser);
                await _userRepository.SaveChangesAsync();

                // Map Admin user to Admin role via UserRole mapping
                var userRoleInfo = new UserRole
                {
                    UserId = (long)adminUserId,
                    RoleId = (int)adminRoleId,
                    TenantId = tenant.Id,
                    CreatedBy = sessionUserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _userRoleRepository.InsertAndGetIdAsync(userRoleInfo);

                await transaction.CommitAsync();

                return new TenantDto { Id = tenant.Id, Name = tenant.Name };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<TenantDto>> GetAllTenantsAsync()
        {
            var tenants = await _tenantRepository.GetAllAsync();
            return tenants.Select(t => new TenantDto { Id = t.Id, Name = t.Name }).ToList();
        }

        public async Task<TenantDto> GetTenantByIdAsync(long id)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null) return null; // or throw NotFoundException depending on your error handling

            return new TenantDto { Id = tenant.Id, Name = tenant.Name };
        }

        public async Task UpdateTenantAsync(long id, UpdateTenantDto dto)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            tenant.Name = dto.Name;
            await _tenantRepository.SaveChangesAsync();
        }

        public async Task DeleteTenantAsync(long id)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            await _tenantRepository.DeleteAsync(tenant);
            await _tenantRepository.SaveChangesAsync();
        }
    }
}