using Microsoft.EntityFrameworkCore;
using backendwork.Models;
namespace backendwork.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRoles> UserRole { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Employee> Employees { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRoles>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<UserRoles>().HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRoles>()
               .HasOne(ur => ur.Role)
               .WithMany(r => r.UserRoles)
               .HasForeignKey(ur => ur.RoleId);
            modelBuilder.Entity<Vendor>()
                .HasOne(v => v.User)
                .WithOne(u => u.Vendor)
                .HasForeignKey<Vendor>(v => v.UserId);
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Vendor)
                .WithMany(v => v.Employees)
                .HasForeignKey(e => e.VendorId);
        }
    }
}