namespace ZuriFluxAPI.Models
{
    public class DeviceToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public DateTime LastUsed { get; set; }

        public User User { get; set; } = null!;
    }
}