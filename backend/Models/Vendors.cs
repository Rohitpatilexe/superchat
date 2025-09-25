using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    // Represents a vendor entity in the database.
    public class Vendor
    {
        // Primary Key for the Vendor table. It is an auto-incrementing integer.
        [Key]
        public int Id { get; set; }

        // A unique, publicly-exposed identifier. Used in API URLs to avoid exposing the internal Id.
        public Guid PublicId { get; set; } = Guid.NewGuid();

        // The company's name. It is a required field.
        [Required]
        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        // The vendor's contact email, which is a required field.
        [Required]
        [MaxLength(256)]
        public string ContactEmail { get; set; } = string.Empty;

        // The country where the vendor is located.
        [Required]
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        // The current status of the vendor (e.g., "Pending", "Verified", "Rejected").
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        // A temporary token used for email verification, if applicable.
        public Guid? VerificationToken { get; set; }
        public DateTime? TokenExpiry { get; set; }

        // Timestamps to track when the record was created and last updated.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys:
        // UserId links to the vendor's own user account. It is nullable because a vendor might not have an account yet.
        public int? UserId { get; set; }

        // AddedByLeaderId links to the leader who added this vendor.
        public int AddedByLeaderId { get; set; }

        // UpdatedByLeaderId links to the last leader who updated the record.
        public int? UpdatedByLeaderId { get; set; }

        // Navigation Properties:
        // This property allows you to access the related User object.
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // This property allows you to access the leader's User object.
        [ForeignKey("AddedByLeaderId")]
        public virtual User AddedByLeader { get; set; } = null!;

        // This collection represents a one-to-many relationship with Employees.
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

        // This collection represents the many-to-many relationship with Job.
        // It's a navigation property that links to the JobVendor join table.
        public virtual ICollection<JobVendor> JobAssignments { get; set; } = new List<JobVendor>();
    }
}