namespace backendwork.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobTitle { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? TerminationDate { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }

        // Foreign Key to Vendors table
        public int VendorId { get; set; }
        
        public Vendor Vendor { get; set; }
    }
}