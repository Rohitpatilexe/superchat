using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }
        public Guid PublicId { get; set; } = Guid.NewGuid();

        // Foreign Keys to link the conversation
        public int JobId { get; set; }
        public int VendorId { get; set; }
        public int LeaderId { get; set; } // The User ID of the leader

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("JobId")]
        public virtual Job Job { get; set; } = null!;

        [ForeignKey("VendorId")]
        public virtual Vendor Vendor { get; set; } = null!;

        [ForeignKey("LeaderId")]
        public virtual User Leader { get; set; } = null!;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}