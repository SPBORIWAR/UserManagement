namespace UserManagement.EntityFrameworkCore.Models
{
    public class User : Entity<long>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PersonalIdentificationNumber { get; set; }
        public string? PersonalIdentificationName { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNo { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ZipCode { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? ProfilePicturePath { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }        
    }
}