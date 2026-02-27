using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;
using ZuriFluxAPI.Repositories;

namespace ZuriFluxAPI.Services
{
    public class BinService : IBinService
    {
        private readonly IBinRepository _binRepository;

        public BinService(IBinRepository binRepository)
        {
            _binRepository = binRepository;
        }

        public async Task<IEnumerable<BinResponseDto>> GetAllBinsAsync()
        {
            var bins = await _binRepository.GetAllAsync();
            return bins.Select(MapToDto);
        }

        public async Task<BinResponseDto> GetBinByIdAsync(int id)
        {
            var bin = await _binRepository.GetByIdAsync(id);
            if (bin == null) return null;
            return MapToDto(bin);
        }

        public async Task<IEnumerable<BinResponseDto>> GetBinsNeedingPickupAsync()
        {
            var bins = await _binRepository.GetBinsNeedingPickupAsync();
            return bins.Select(MapToDto);
        }

        public async Task<BinResponseDto> CreateBinAsync(CreateBinDto dto)
        {
            var bin = new Bin
            {
                Location = dto.Location,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                WasteType = dto.WasteType,
                FillLevel = 0,
                NeedsPickup = false,
                LastUpdated = DateTime.UtcNow
            };

            var created = await _binRepository.CreateAsync(bin);
            return MapToDto(created);
        }

        public async Task ProcessSensorReadingAsync(SensorReadingDto dto)
        {
            // Save the raw reading
            var reading = new SensorReading
            {
                BinId = dto.BinId,
                FillLevel = dto.FillLevel,
                OdorLevel = dto.OdorLevel,
                GasDetected = dto.GasDetected,
                RecordedAt = DateTime.UtcNow
            };
            await _binRepository.AddSensorReadingAsync(reading);

            // Update the bin's current state
            var bin = await _binRepository.GetByIdAsync(dto.BinId);
            if (bin != null)
            {
                bin.FillLevel = dto.FillLevel;
                bin.LastUpdated = DateTime.UtcNow;

                // Business rule: if fill level is 80%+ flag it for pickup
                bin.NeedsPickup = dto.FillLevel >= 80;

                await _binRepository.UpdateAsync(bin);
            }
        }

        // Private helper to map a Bin model to a BinResponseDto
        private BinResponseDto MapToDto(Bin bin) => new BinResponseDto
        {
            Id = bin.Id,
            Location = bin.Location,
            Latitude = bin.Latitude,
            Longitude = bin.Longitude,
            FillLevel = bin.FillLevel,
            WasteType = bin.WasteType,
            NeedsPickup = bin.NeedsPickup,
            LastUpdated = bin.LastUpdated
        };

        public async Task<PagedResult<BinResponseDto>> GetPagedBinsAsync(BinFilterParams filters)
        {
            var pagedBins = await _binRepository.GetPagedAsync(filters);

            return new PagedResult<BinResponseDto>
            {
                Data = pagedBins.Data.Select(MapToDto),
                PageNumber = pagedBins.PageNumber,
                PageSize = pagedBins.PageSize,
                TotalRecords = pagedBins.TotalRecords,
                TotalPages = pagedBins.TotalPages,
                HasNextPage = pagedBins.HasNextPage,
                HasPreviousPage = pagedBins.HasPreviousPage
            };
        }
    }
}
