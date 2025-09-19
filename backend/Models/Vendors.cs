using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Vendor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string CompanyName { get; set; }

        [Required]
        [MaxLength(256)]
        public string ContactEmail { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        public Guid? VerificationToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
    
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        public int? UserId { get; set; } // The vendor's own user account
        public int AddedByAdminId { get; set; } // The admin who added the vendor
        public int? UpdatedByAdminId { get; set; }
    
        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("AddedByAdminId")]
        public virtual User AddedByAdmin { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}