using Microsoft.EntityFrameworkCore;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Helper;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Repositories
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly ZuriFluxDbContext _context;

        public CollectionRepository(ZuriFluxDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WasteCollection>> GetAllAsync()
        {
            return await _context.WasteCollections
                .Include(w => w.Bin)          // bring in bin details
                .Include(w => w.Collector)    // bring in collector details
                .OrderByDescending(w => w.CollectedAt)  // newest first
                .ToListAsync();
        }

        public async Task<IEnumerable<WasteCollection>> GetByCollectorIdAsync(int collectorId)
        {
            return await _context.WasteCollections
                .Include(w => w.Bin)
                .Include(w => w.Collector)
                .Where(w => w.CollectorId == collectorId)
                .OrderByDescending(w => w.CollectedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<WasteCollection>> GetByBinIdAsync(int binId)
        {
            return await _context.WasteCollections
                .Include(w => w.Bin)
                .Include(w => w.Collector)
                .Where(w => w.BinId == binId)
                .OrderByDescending(w => w.CollectedAt)
                .ToListAsync();
        }

        public async Task<WasteCollection> GetByIdAsync(int id)
        {
            return await _context.WasteCollections
                .Include(w => w.Bin)
                .Include(w => w.Collector)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<WasteCollection> CreateAsync(WasteCollection collection)
        {
            _context.WasteCollections.Add(collection);
            await _context.SaveChangesAsync();
            return collection;
        }

        public async Task UpdateAsync(WasteCollection collection)
        {
            _context.WasteCollections.Update(collection);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<WasteCollection>> GetPagedAsync(CollectionFilterParams filters)
        {
            var query = _context.WasteCollections
                .Include(w => w.Bin)
                .Include(w => w.Collector)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filters.Status))
                query = query.Where(w => w.Status == filters.Status);

            if (filters.CollectorId.HasValue)
                query = query.Where(w => w.CollectorId == filters.CollectorId.Value);

            if (filters.FromDate.HasValue)
                query = query.Where(w => w.CollectedAt >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(w => w.CollectedAt <= filters.ToDate.Value);

            // Newest first
            query = query.OrderByDescending(w => w.CollectedAt);

            return await PaginationHelper.CreatePagedResultAsync(
                query, filters.PageNumber, filters.PageSize);
        }
    }
}
