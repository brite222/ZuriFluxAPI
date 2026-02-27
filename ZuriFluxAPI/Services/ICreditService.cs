using ZuriFluxAPI.DTOs;

namespace ZuriFluxAPI.Services
{
    public interface ICreditService
    {
        Task<PagedResult<CreditTransactionResponseDto>> GetPagedAsync(CreditFilterParams filters);  // ← add
        Task<CreditTransactionResponseDto> AwardCreditsAsync(AwardCreditDto dto);
        Task<CreditTransactionResponseDto> DeductCreditsAsync(DeductCreditDto dto);
        Task<CreditSummaryDto> GetUserCreditSummaryAsync(int userId);
        Task<IEnumerable<CreditTransactionResponseDto>> GetAllTransactionsAsync();
        Task<IEnumerable<CreditTransactionResponseDto>> GetUserTransactionsAsync(int userId);
    }
}
