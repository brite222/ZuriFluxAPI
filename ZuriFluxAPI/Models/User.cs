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

        // Referral fields
        public string ReferralCode { get; set; }    // unique code e.g. "TUNDE123"
        public int? ReferredByUserId { get; set; }  // who referred this user
        public int TotalReferrals { get; set; }
        // Navigation
        public User ReferredBy { get; set; }

    }
}
