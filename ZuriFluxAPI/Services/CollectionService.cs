using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;
using ZuriFluxAPI.Repositories;

namespace ZuriFluxAPI.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly IBinRepository _binRepository;
        private readonly IUserRepository _userRepository;

        public CollectionService(
            ICollectionRepository collectionRepository,
            IBinRepository binRepository,
            IUserRepository userRepository)
        {
            _collectionRepository = collectionRepository;
            _binRepository = binRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<CollectionResponseDto>> GetAllAsync()
        {
            var collections = await _collectionRepository.GetAllAsync();
            return collections.Select(MapToDto);
        }

        public async Task<IEnumerable<CollectionResponseDto>> GetByCollectorIdAsync(int collectorId)
        {
            var collections = await _collectionRepository.GetByCollectorIdAsync(collectorId);
            return collections.Select(MapToDto);
        }

        public async Task<IEnumerable<CollectionResponseDto>> GetByBinIdAsync(int binId)
        {
            var collections = await _collectionRepository.GetByBinIdAsync(binId);
            return collections.Select(MapToDto);
        }

        public async Task<CollectionResponseDto> LogCollectionAsync(CreateCollectionDto dto)
        {
            // Make sure the bin exists
            var bin = await _binRepository.GetByIdAsync(dto.BinId);
            if (bin == null)
                throw new Exception($"Bin with ID {dto.BinId} not found.");

            // Make sure the collector exists and is actually a collector
            var collector = await _userRepository.GetByIdAsync(dto.CollectorId);
            if (collector == null)
                throw new Exception($"Collector with ID {dto.CollectorId} not found.");
            if (collector.Role != "collector" && collector.Role != "admin")
                throw new Exception("This user is not authorized to log collections.");

            // Create the collection record
            var collection = new WasteCollection
            {
                BinId = dto.BinId,
                CollectorId = dto.CollectorId,
                CollectedAt = DateTime.UtcNow,
                Status = "completed",
                Notes = dto.Notes
            };

            var created = await _collectionRepository.CreateAsync(collection);

            // Reset the bin after successful pickup
            bin.FillLevel = 0;
            bin.NeedsPickup = false;
            bin.LastUpdated = DateTime.UtcNow;
            await _binRepository.UpdateAsync(bin);

            // Award the collector 10 credits for the pickup
            collector.CreditBalance += 10;
            await _userRepository.UpdateAsync(collector);

            // Reload to get navigation properties for the response
            var result = await _collectionRepository.GetByIdAsync(created.Id);
            return MapToDto(result);
        }

        public async Task<CollectionResponseDto> UpdateStatusAsync(int id, UpdateCollectionStatusDto dto)
        {
            var collection = await _collectionRepository.GetByIdAsync(id);
            if (collection == null)
                throw new Exception($"Collection with ID {id} not found.");

            var validStatuses = new[] { "completed", "failed", "pending" };
            if (!validStatuses.Contains(dto.Status))
                throw new Exception("Invalid status. Use: completed, failed, or pending.");

            collection.Status = dto.Status;
            await _collectionRepository.UpdateAsync(collection);

            return MapToDto(collection);
        }

        private CollectionResponseDto MapToDto(WasteCollection w) => new CollectionResponseDto
        {
            Id = w.Id,
            BinId = w.BinId,
            BinLocation = w.Bin?.Location,
            CollectorName = w.Collector?.FullName,
            Status = w.Status,
            Notes = w.Notes,
            CollectedAt = w.CollectedAt
        };



        public async Task<PagedResult<CollectionResponseDto>> GetPagedAsync(CollectionFilterParams filters)
        {
            var pagedCollections = await _collectionRepository.GetPagedAsync(filters);

            return new PagedResult<CollectionResponseDto>
            {
                Data = pagedCollections.Data.Select(MapToDto),
                PageNumber = pagedCollections.PageNumber,
                PageSize = pagedCollections.PageSize,
                TotalRecords = pagedCollections.TotalRecords,
                TotalPages = pagedCollections.TotalPages,
                HasNextPage = pagedCollections.HasNextPage,
                HasPreviousPage = pagedCollections.HasPreviousPage
            };
        }
    }
}
