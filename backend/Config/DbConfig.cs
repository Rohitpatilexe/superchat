using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Config
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Define a DbSet for each table you want to interact with
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobVendor> JobVendors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Leadership" },
                new Role { Id = 3, Name = "Vendor" }
            );

            // --- Configure the new JobVendor many-to-many relationship ---
            modelBuilder.Entity<JobVendor>()
                .HasKey(jv => new { jv.JobId, jv.VendorId });

            // JobVendor -> Job
            modelBuilder.Entity<JobVendor>()
                .HasOne(jv => jv.Job)
                .WithMany(j => j.VendorAssignments)
                .HasForeignKey(jv => jv.JobId);

            // JobVendor -> Vendor
            modelBuilder.Entity<JobVendor>()
                .HasOne(jv => jv.Vendor)
                .WithMany(v => v.JobAssignments)
                .HasForeignKey(jv => jv.VendorId);

            // --- Configure the new one-to-many relationship between Job and Employee ---
            modelBuilder.Entity<Job>()
                .HasMany(j => j.Employees)
                .WithOne(e => e.Job)
                .HasForeignKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the many-to-many relationship between User and Role
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity(j => j.ToTable("UserRoles"));

            // Configure the one-to-many relationship between Vendor and Employee
            modelBuilder.Entity<Vendor>()
                .HasMany(v => v.Employees)
                .WithOne(e => e.Vendor)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            // to break cascade cycle
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the one-to-one relationship between a Vendor and their User account
            modelBuilder.Entity<Vendor>()
                .HasOne(v => v.User)
                .WithOne()
                .HasForeignKey<Vendor>(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vendor>()
            .HasOne(v => v.AddedByLeader)
            .WithMany()
            .HasForeignKey(v => v.AddedByLeaderId)
            .OnDelete(DeleteBehavior.Restrict);

            // To ensure fast lookups and prevent duplicate GUIDs, adding a unique index to the new PublicId columns.
            modelBuilder.Entity<User>().HasIndex(u => u.PublicId).IsUnique();
            modelBuilder.Entity<Vendor>().HasIndex(v => v.PublicId).IsUnique();
            modelBuilder.Entity<Job>().HasIndex(j => j.PublicId).IsUnique();
            modelBuilder.Entity<Employee>().HasIndex(e => e.PublicId).IsUnique();
        }
    }
}