using ZuriFluxAPI.DTOs;

namespace ZuriFluxAPI.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<BinAnalyticsDto> GetBinAnalyticsAsync();
        Task<CollectionAnalyticsDto> GetCollectionAnalyticsAsync();
        Task<CreditAnalyticsDto> GetCreditAnalyticsAsync();
        Task<LeaderboardDto> GetLeaderboardAsync(int topCount);
    }
}
