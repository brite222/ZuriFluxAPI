using ZuriFluxAPI.DTOs;

namespace ZuriFluxAPI.Services
{
    public interface IBinService
    {
        Task<IEnumerable<BinResponseDto>> GetAllBinsAsync();
        Task<BinResponseDto> GetBinByIdAsync(int id);
        Task<IEnumerable<BinResponseDto>> GetBinsNeedingPickupAsync();
        Task<BinResponseDto> CreateBinAsync(CreateBinDto dto);
        Task<PagedResult<BinResponseDto>> GetPagedBinsAsync(BinFilterParams filters);  
        Task ProcessSensorReadingAsync(SensorReadingDto dto);
    }
}
