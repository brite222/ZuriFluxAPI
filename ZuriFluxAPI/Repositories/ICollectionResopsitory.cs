using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Repositories
{
    public interface ICollectionRepository
    {
        Task<IEnumerable<WasteCollection>> GetAllAsync();
        Task<PagedResult<WasteCollection>> GetPagedAsync(CollectionFilterParams filters);  // ← new
        Task<IEnumerable<WasteCollection>> GetByCollectorIdAsync(int collectorId);
        Task<IEnumerable<WasteCollection>> GetByBinIdAsync(int binId);
        Task<WasteCollection> GetByIdAsync(int id);
        Task<WasteCollection> CreateAsync(WasteCollection collection);
        Task UpdateAsync(WasteCollection collection);
    }
}
