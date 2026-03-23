namespace ZuriFluxAPI.DTOs
{
    // Overall system dashboard summary
    public class DashboardSummaryDto
    {
        public int TotalBins { get; set; }
        public int BinsNeedingPickup { get; set; }
        public int BinsOperational { get; set; }
        public double AverageFillLevel { get; set; }
        public int TotalCollectionsToday { get; set; }
        public int TotalCollectionsThisMonth { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TotalCreditsAwarded { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    // Breakdown of bin health across locations
    public class BinAnalyticsDto
    {
        public int TotalBins { get; set; }
        public int FullBins { get; set; }           // 80%+ fill level
        public int ModerateBins { get; set; }        // 40-79% fill level
        public int EmptyBins { get; set; }           // below 40%
        public List<BinHotspotDto> Hotspots { get; set; }  // busiest locations
    }

    // A single hotspot location
    public class BinHotspotDto
    {
        public int BinId { get; set; }
        public string Location { get; set; }
        public int FillLevel { get; set; }
        public int TotalReadings { get; set; }      // how active this bin is
    }

    // Collection performance stats
    public class CollectionAnalyticsDto
    {
        public int TotalCollections { get; set; }
        public int CompletedCollections { get; set; }
        public int FailedCollections { get; set; }
        public double CompletionRate { get; set; }   // percentage
        public List<TopCollectorDto> TopCollectors { get; set; }
    }

    // A single top collector entry
    public class TopCollectorDto
    {
        public int CollectorId { get; set; }
        public string CollectorName { get; set; }
        public int TotalPickups { get; set; }
        public int CreditsEarned { get; set; }
    }

    // Credit system overview
    public class CreditAnalyticsDto
    {
        public int TotalCreditsAwarded { get; set; }
        public int TotalCreditsDeducted { get; set; }
        public int TotalCreditsInCirculation { get; set; }
        public int TotalUsersWithCredits { get; set; }
        public List<TopCitizenDto> TopCitizens { get; set; }  // most credits earned
    }

    // A single top citizen entry
    public class TopCitizenDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CreditBalance { get; set; }
        public int TotalTransactions { get; set; }
    }

    // Full leaderboard response
    public class LeaderboardDto
    {
        public List<CitizenLeaderboardDto> TopCitizens { get; set; }
        public List<CollectorLeaderboardDto> TopCollectors { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    // Single citizen entry
    public class CitizenLeaderboardDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int CreditBalance { get; set; }
        public int TotalReferrals { get; set; }
        public DateTime MemberSince { get; set; }
    }

    // Single collector entry
    public class CollectorLeaderboardDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int TotalPickups { get; set; }
        public int ScheduledPickups { get; set; }
        public int TotalCreditsEarned { get; set; }
    }
}
