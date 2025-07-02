namespace UserManagement.BusinessLogic.SessionManagment
{
    public interface ISessionService
    {
        long UserId { get; }
        string UserName { get; }
        int TenantId { get; }
        int RoleId { get; }
        List<string> Roles { get; }
        string PrimaryRole { get; }
    }
}