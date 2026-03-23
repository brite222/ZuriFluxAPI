using Microsoft.EntityFrameworkCore;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Helper;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ZuriFluxDbContext _context;

        public ScheduleRepository(ZuriFluxDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CollectionSchedule>> GetPagedAsync(
            ScheduleFilterParams filters)
        {
            var query = _context.CollectionSchedules
                .Include(cs => cs.Bin)
                .Include(cs => cs.RequestedBy)
                .Include(cs => cs.AssignedCollector)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filters.Status))
                query = query.Where(cs => cs.Status == filters.Status);

            if (!string.IsNullOrEmpty(filters.TimeSlot))
                query = query.Where(cs => cs.TimeSlot == filters.TimeSlot);

            if (filters.FromDate.HasValue)
                query = query.Where(cs => cs.ScheduledDate >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(cs => cs.ScheduledDate <= filters.ToDate.Value);

            if (filters.CollectorId.HasValue)
                query = query.Where(cs =>
                    cs.AssignedCollectorId == filters.CollectorId.Value);

            if (filters.UserId.HasValue)
                query = query.Where(cs =>
                    cs.RequestedByUserId == filters.UserId.Value);

            query = query.OrderBy(cs => cs.ScheduledDate);

            return await PaginationHelper.CreatePagedResultAsync(
                query, filters.PageNumber, filters.PageSize);
        }

        public async Task<CollectionSchedule> GetByIdAsync(int id)
        {
            return await _context.CollectionSchedules
                .Include(cs => cs.Bin)
                .Include(cs => cs.RequestedBy)
                .Include(cs => cs.AssignedCollector)
                .FirstOrDefaultAsync(cs => cs.Id == id);
        }

        public async Task<CollectionSchedule> CreateAsync(CollectionSchedule schedule)
        {
            _context.CollectionSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task UpdateAsync(CollectionSchedule schedule)
        {
            _context.CollectionSchedules.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasConflictAsync(
            int binId, DateTime scheduledDate, string timeSlot)
        {
            // Check if this bin already has a pending/accepted schedule
            // for the same date and time slot
            return await _context.CollectionSchedules
                .AnyAsync(cs =>
                    cs.BinId == binId &&
                    cs.ScheduledDate.Date == scheduledDate.Date &&
                    cs.TimeSlot == timeSlot &&
                    (cs.Status == "pending" || cs.Status == "accepted"));
        }
    }
}
