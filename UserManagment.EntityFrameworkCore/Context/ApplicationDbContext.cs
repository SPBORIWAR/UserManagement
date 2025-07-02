using Microsoft.EntityFrameworkCore;
using UserManagement.EntityFrameworkCore.Models;

namespace UserManagement.EntityFrameworkCore.Context
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ThemeSetting> ThemeSettings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Language> Languages { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("UserManagement");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<User>()
            //    .HasOne(u => u.Role)
            //    .WithMany(r => r.Users)
            //    .HasForeignKey(u => u.RoleId)
            //    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Role>()
                .HasOne(r => r.Tenant)
                .WithMany(t => t.Roles)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Tenant)
                .WithMany()
                .HasForeignKey(rp => rp.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.Tenant)
                .WithMany(t => t.Permissions)
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => ur.Id);

            // ❗ Fixed to avoid multiple cascade paths
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict); // 👈 Fixed here

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Tenant)
                .WithMany()
                .HasForeignKey(ur => ur.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasKey(al => al.Id);

            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.Tenant)
                .WithMany()
                .HasForeignKey(al => al.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ThemeSetting>()
                .HasOne(ts => ts.User)
                .WithMany()
                .HasForeignKey(ts => ts.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ThemeSetting>()
                .HasOne(ts => ts.Tenant)
                .WithMany()
                .HasForeignKey(ts => ts.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.TargetUser)
                .WithMany()
                .HasForeignKey(n => n.TargetUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Tenant)
                .WithMany()
                .HasForeignKey(n => n.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.FromUser)
                .WithMany()
                .HasForeignKey(c => c.FromUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.ToUser)
                .WithMany()
                .HasForeignKey(c => c.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Tenant)
                .WithMany()
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Language>()
                .HasKey(l => l.Id);
        }
    }
}