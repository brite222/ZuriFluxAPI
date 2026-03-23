using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Repositories
{
    public interface IScheduleRepository
    {
        Task<PagedResult<CollectionSchedule>> GetPagedAsync(ScheduleFilterParams filters);
        Task<CollectionSchedule> GetByIdAsync(int id);
        Task<CollectionSchedule> CreateAsync(CollectionSchedule schedule);
        Task UpdateAsync(CollectionSchedule schedule);
        Task<bool> HasConflictAsync(int binId, DateTime scheduledDate, string timeSlot);
    }
}
