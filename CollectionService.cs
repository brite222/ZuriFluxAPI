ZuriFluxAPI\Services\CollectionService.cs
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;
using ZuriFluxAPI.Repositories;
using System.Linq;

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

        // New: paged query implemented in-memory using repository data.
        public async Task<PagedResult<CollectionResponseDto>> GetPagedAsync(CollectionFilterParams filters)
        {
            var allCollections = await _collectionRepository.GetAllAsync(); // includes Bin & Collector
            var query = allCollections.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Status))
                query = query.Where(w => w.Status == filters.Status);

            if (filters.FromDate.HasValue)
                query = query.Where(w => w.CollectedAt >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(w => w.CollectedAt <= filters.ToDate.Value);

            if (filters.CollectorId.HasValue)
                query = query.Where(w => w.CollectorId == filters.CollectorId.Value);

            var totalRecords = query.Count();

            var pageNumber = Math.Max(1, filters.PageNumber);
            var pageSize = filters.PageSize;

            var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 1;

            var items = query
                .OrderByDescending(w => w.CollectedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            var result = new PagedResult<CollectionResponseDto>
            {
                Data = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };

            return result;
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
    }
}