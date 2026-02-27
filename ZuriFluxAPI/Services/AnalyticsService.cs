using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Repositories;

namespace ZuriFluxAPI.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        public AnalyticsService(IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
            => await _analyticsRepository.GetDashboardSummaryAsync();

        public async Task<BinAnalyticsDto> GetBinAnalyticsAsync()
            => await _analyticsRepository.GetBinAnalyticsAsync();

        public async Task<CollectionAnalyticsDto> GetCollectionAnalyticsAsync()
            => await _analyticsRepository.GetCollectionAnalyticsAsync();

        public async Task<CreditAnalyticsDto> GetCreditAnalyticsAsync()
            => await _analyticsRepository.GetCreditAnalyticsAsync();
    }
}
