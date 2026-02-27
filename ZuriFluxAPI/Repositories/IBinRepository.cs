using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Repositories
{
    public interface IBinRepository
    {
        Task<IEnumerable<Bin>> GetAllAsync();
        Task<PagedResult<Bin>> GetPagedAsync(BinFilterParams filters);  
        Task<Bin> GetByIdAsync(int id);
        Task<IEnumerable<Bin>> GetBinsNeedingPickupAsync();
        Task<Bin> CreateAsync(Bin bin);
        Task UpdateAsync(Bin bin);
        Task AddSensorReadingAsync(SensorReading reading);
    }
}
