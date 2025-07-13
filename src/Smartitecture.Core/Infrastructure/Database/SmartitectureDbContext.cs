using Microsoft.EntityFrameworkCore;
using Smartitecture.Core.Security.Models;

namespace Smartitecture.Core.Infrastructure.Database
{
    public class SmartitectureDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        public SmartitectureDbContext(DbContextOptions<SmartitectureDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            // Configure relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<UserRole>(
                    "UserRoles",
                    ur => ur.HasOne<Role>().WithMany(),
                    ur => ur.HasOne<User>().WithMany(),
                    ur =>
                    {
                        ur.HasKey(ur => new { ur.UserId, ur.RoleId });
                        ur.ToTable("UserRoles");
                    });

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity<RolePermission>(
                    "RolePermissions",
                    rp => rp.HasOne<Permission>().WithMany(),
                    rp => rp.HasOne<Role>().WithMany(),
                    rp =>
                    {
                        rp.HasKey(rp => new { rp.RoleId, rp.PermissionId });
                        rp.ToTable("RolePermissions");
                    });
        }
    }
}
