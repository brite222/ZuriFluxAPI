using ZuriFluxAPI.DTOs;

namespace ZuriFluxAPI.Services
{
    public interface ICollectionService
    {
        Task<PagedResult<CollectionResponseDto>> GetPagedAsync(CollectionFilterParams filters);  
        Task<IEnumerable<CollectionResponseDto>> GetAllAsync();
        Task<IEnumerable<CollectionResponseDto>> GetByCollectorIdAsync(int collectorId);
        Task<IEnumerable<CollectionResponseDto>> GetByBinIdAsync(int binId);
        Task<CollectionResponseDto> LogCollectionAsync(CreateCollectionDto dto);
        Task<CollectionResponseDto> UpdateStatusAsync(int id, UpdateCollectionStatusDto dto);
    }
}
