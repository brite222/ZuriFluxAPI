namespace ZuriFluxAPI.Models
{
    public class CreditTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime TransactedAt { get; set; }
        public User User { get; set; } = null!;
    }
}