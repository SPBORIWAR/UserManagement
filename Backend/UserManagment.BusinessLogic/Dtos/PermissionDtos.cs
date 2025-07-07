namespace UserManagement.BusinessLogic.Dtos
{
    public class PermissionDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int TenantId { get; set; }
    }

    public class CreatePermissionDto
    {
        public string Name { get; set; }
        public int? TenantId { get; set; } // Optional — if null, fallback to session.TenantId
    }

    public class UpdatePermissionDto
    {
        public string Name { get; set; }
    }

    public class AssignPermissionDto
    {
        public int RoleId { get; set; }
        public long PermissionId { get; set; }
    }

    public class RemovePermissionDto
    {
        public int RoleId { get; set; }
        public long PermissionId { get; set; }
    }

    public class RolePermissionDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public List<string> Permissions { get; set; } = new();
    }

    public class PermissionListByRoleDto
    {
        public int RoleId { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}