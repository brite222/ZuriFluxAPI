using Microsoft.EntityFrameworkCore;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Helper;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Repositories
{
    public class BinRepository : IBinRepository
    {
        private readonly ZuriFluxDbContext _context;

        public BinRepository(ZuriFluxDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Bin>> GetAllAsync()
        {
            return await _context.Bins.ToListAsync();
        }

        public async Task<Bin> GetByIdAsync(int id)
        {
            return await _context.Bins
                .Include(b => b.SensorReadings)  // also load related readings
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Bin>> GetBinsNeedingPickupAsync()
        {
            return await _context.Bins
                .Where(b => b.NeedsPickup == true)
                .ToListAsync();
        }

        public async Task<Bin> CreateAsync(Bin bin)
        {
            _context.Bins.Add(bin);
            await _context.SaveChangesAsync();
            return bin;
        }

        public async Task UpdateAsync(Bin bin)
        {
            _context.Bins.Update(bin);
            await _context.SaveChangesAsync();
        }

        public async Task AddSensorReadingAsync(SensorReading reading)
        {
            _context.SensorReadings.Add(reading);
            await _context.SaveChangesAsync();
        }
        public async Task<PagedResult<Bin>> GetPagedAsync(BinFilterParams filters)
        {
            // Start with all bins as a queryable (nothing hits DB yet)
            var query = _context.Bins.AsQueryable();

            // Apply filters only if they were provided
            if (!string.IsNullOrEmpty(filters.Location))
                query = query.Where(b =>
                    b.Location.ToLower().Contains(filters.Location.ToLower()));

            if (!string.IsNullOrEmpty(filters.WasteType))
                query = query.Where(b => b.WasteType == filters.WasteType);

            if (filters.NeedsPickup.HasValue)
                query = query.Where(b => b.NeedsPickup == filters.NeedsPickup.Value);

            if (filters.MinFillLevel.HasValue)
                query = query.Where(b => b.FillLevel >= filters.MinFillLevel.Value);

            // Order consistently before paginating
            query = query.OrderBy(b => b.Id);

            return await PaginationHelper.CreatePagedResultAsync(
                query, filters.PageNumber, filters.PageSize);
        }
    }
}
