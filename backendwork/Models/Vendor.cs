namespace backendwork.Models
{
    public class Vendor
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string ContactEmail { get; set; }
        public string Status { get; set; }
        public Guid? VerificationToken { get; set; }
        public DateTime? TokenExpire { get; set; }
        public string InternalNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int AddedByAdminId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByAdminId { get; set; }

        // Foreign Key to Users table
        public int UserId { get; set; }  
        
        public User User { get; set; }
        public List<Employee> Employees { get; set; }
    }
}