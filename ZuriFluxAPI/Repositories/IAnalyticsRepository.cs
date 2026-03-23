using ZuriFluxAPI.DTOs;

namespace ZuriFluxAPI.Repositories
{
    public interface IAnalyticsRepository
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<BinAnalyticsDto> GetBinAnalyticsAsync();
        Task<CollectionAnalyticsDto> GetCollectionAnalyticsAsync();
        Task<CreditAnalyticsDto> GetCreditAnalyticsAsync();
        Task<LeaderboardDto> GetLeaderboardAsync(int topCount);
    }
}
