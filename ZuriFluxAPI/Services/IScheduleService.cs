using ZuriFluxAPI.DTOs;

namespace ZuriFluxAPI.Services
{
    public interface IScheduleService
    {
        Task<PagedResult<ScheduleResponseDto>> GetPagedAsync(ScheduleFilterParams filters);
        Task<ScheduleResponseDto> GetByIdAsync(int id);
        Task<ScheduleResponseDto> CreateScheduleAsync(CreateScheduleDto dto, int requestedByUserId);
        Task<ScheduleResponseDto> AssignCollectorAsync(int scheduleId, AssignCollectorDto dto);
        Task<ScheduleResponseDto> UpdateStatusAsync(int scheduleId, UpdateScheduleStatusDto dto, int updatedByUserId);
    }
}
