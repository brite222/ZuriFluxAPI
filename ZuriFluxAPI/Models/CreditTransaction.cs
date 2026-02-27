namespace ZuriFluxAPI.Models
{
    public class CreditTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Amount { get; set; }             // positive = earned, negative = spent
        public string Reason { get; set; }          // "proper disposal", "redeemed reward"
        public DateTime TransactedAt { get; set; }

        public User User { get; set; }
    }
}
