namespace ZuriFluxAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }            // "citizen", "admin", "collector"
        public int CreditBalance { get; set; }      // waste tokens earned
        public DateTime CreatedAt { get; set; }
    }
}
