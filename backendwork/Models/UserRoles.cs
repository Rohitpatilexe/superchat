namespace backendwork.Models
{
    public class UserRoles
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        // Navigation Properties
        public User User { get; set; }
        public Role Role { get; set; }
    }
}