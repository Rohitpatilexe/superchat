namespace backendwork.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool isActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public List<UserRoles> UserRole { get; set; }
        public Vendor Vendor { get; set; }
    }
}