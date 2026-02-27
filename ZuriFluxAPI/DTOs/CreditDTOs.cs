namespace ZuriFluxAPI.DTOs
{
    // What admin sends to award credits to a citizen
    public class AwardCreditDto
    {
        public int UserId { get; set; }
        public int Amount { get; set; }
        public string Reason { get; set; }  // "proper disposal", "recycling bonus", etc.
    }

    // What admin sends to deduct credits (e.g. illegal dumping penalty)
    public class DeductCreditDto
    {
        public int UserId { get; set; }
        public int Amount { get; set; }
        public string Reason { get; set; }  // "illegal dumping penalty"
    }

    // What the API returns for each transaction
    public class CreditTransactionResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Amount { get; set; }          // positive = earned, negative = deducted
        public string Reason { get; set; }
        public DateTime TransactedAt { get; set; }
    }

    // Summary of a user's credit status
    public class CreditSummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CurrentBalance { get; set; }
        public int TotalEarned { get; set; }
        public int TotalDeducted { get; set; }
        public List<CreditTransactionResponseDto> RecentTransactions { get; set; }
    }
}
