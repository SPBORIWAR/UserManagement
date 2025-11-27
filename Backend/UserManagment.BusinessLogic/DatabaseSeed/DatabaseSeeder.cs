using Microsoft.AspNetCore.Identity;
using UserManagement.EntityFrameworkCore.Models;
using UserManagement.EntityFrameworkCore.Repository;

namespace UserManagement.BusinessLogic.DatabaseSeed
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IRepository<RolePermission> _rolePermissionRepository;

        public DatabaseSeeder(IRepository<Tenant> tenantRepository,
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IRepository<UserRole> userRoleRepository,
            IRepository<Permission> permissionRepository,
            IRepository<RolePermission> rolePermissionRepository)
        {
            _tenantRepository = tenantRepository;
            _passwordHasher = passwordHasher;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _permissionRepository = permissionRepository;
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task SeedAsync()
        {
            if (!await _tenantRepository.AnyAsync(t => t.Name == "Default"))
            {
                var defaultTenant = new Tenant { Name = "Default", CreatedDate = DateTime.UtcNow };
                var tenantId = await _tenantRepository.InsertAndGetIdAsync(defaultTenant);

                var superAdminRole = new Role { Name = "SuperAdmin", TenantId = (int)tenantId, CreatedDate = DateTime.UtcNow };
                var roleId = await _roleRepository.InsertAndGetIdAsync(superAdminRole);

                var superAdminUser = new User
                {
                    UserName = "superadmin",
                    FirstName = "Super",
                    LastName = "Admin",
                    TenantId = (int)tenantId
                    // ❌ Remove RoleId = (int)roleId;
                };
                superAdminUser.Password = _passwordHasher.HashPassword(superAdminUser, "SuperAdmin@123");
                var userId = await _userRepository.InsertAndGetIdAsync(superAdminUser);

                // Map user to role via UserRole mapping table
                await _userRoleRepository.InsertAsync(new UserRole
                {
                    UserId = (long)userId,
                    RoleId = (int)roleId,
                    TenantId = (int)tenantId
                });
                await _userRoleRepository.SaveChangesAsync();

                // Seed Permissions
                var permissions = new List<Permission>
                {
                    new Permission { Name = "Pages", TenantId = (int)tenantId },
                    new Permission { Name = "Pages.Dashboard", TenantId = (int)tenantId },
                    new Permission { Name = "Pages.Administration", TenantId = (int)tenantId },
                    new Permission { Name = "Pages.Administration.Tenant", TenantId = (int)tenantId },
                    new Permission { Name = "Pages.Administration.Roles", TenantId = (int)tenantId },
                    new Permission { Name = "Pages.Administration.Permission", TenantId = (int)tenantId }
                };

                foreach (var permission in permissions)
                {
                    await _permissionRepository.InsertAsync(permission);
                }
                await _permissionRepository.SaveChangesAsync();

                foreach (var permission in permissions)
                {
                    await _rolePermissionRepository.InsertAsync(new RolePermission
                    {
                        RoleId = (int)roleId,
                        PermissionId = permission.Id,
                        TenantId = (int)tenantId,
                        CreatedDate = DateTime.UtcNow
                    });
                }
                await _rolePermissionRepository.SaveChangesAsync();
            }
        }
    }
}