using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Repositories
{
    public interface ICreditRepository
    {
        Task<IEnumerable<CreditTransaction>> GetAllAsync();
        Task<PagedResult<CreditTransaction>> GetPagedAsync(CreditFilterParams filters);  // ← new
        Task<IEnumerable<CreditTransaction>> GetByUserIdAsync(int userId);
        Task<CreditTransaction> GetByIdAsync(int id);
        Task<CreditTransaction> CreateAsync(CreditTransaction transaction);
    }
}
