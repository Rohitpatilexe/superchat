namespace backendwork.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigation Property
        public List<UserRoles> UserRoles { get; set; }
    }
}