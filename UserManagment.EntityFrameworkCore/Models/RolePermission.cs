namespace UserManagement.EntityFrameworkCore.Models
{
    public class RolePermission : Entity<long>
    {
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public long PermissionId { get; set; }
        public Permission Permission { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}