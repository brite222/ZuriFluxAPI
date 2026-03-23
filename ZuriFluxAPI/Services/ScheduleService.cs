using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Models;
using ZuriFluxAPI.Repositories;

namespace ZuriFluxAPI.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IBinRepository _binRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICreditRepository _creditRepository;

        public ScheduleService(
            IScheduleRepository scheduleRepository,
            IBinRepository binRepository,
            IUserRepository userRepository,
            ICreditRepository creditRepository)
        {
            _scheduleRepository = scheduleRepository;
            _binRepository = binRepository;
            _userRepository = userRepository;
            _creditRepository = creditRepository;
        }

        public async Task<PagedResult<ScheduleResponseDto>> GetPagedAsync(
            ScheduleFilterParams filters)
        {
            var paged = await _scheduleRepository.GetPagedAsync(filters);
            return new PagedResult<ScheduleResponseDto>
            {
                Data = paged.Data.Select(MapToDto),
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                TotalRecords = paged.TotalRecords,
                TotalPages = paged.TotalPages,
                HasNextPage = paged.HasNextPage,
                HasPreviousPage = paged.HasPreviousPage
            };
        }

        public async Task<ScheduleResponseDto> GetByIdAsync(int id)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null) return null;
            return MapToDto(schedule);
        }

        public async Task<ScheduleResponseDto> CreateScheduleAsync(
            CreateScheduleDto dto, int requestedByUserId)
        {
            // Make sure bin exists
            var bin = await _binRepository.GetByIdAsync(dto.BinId);
            if (bin == null)
                throw new Exception($"Bin with ID {dto.BinId} not found.");

            // Can't book in the past
            if (dto.ScheduledDate.Date < DateTime.UtcNow.Date)
                throw new Exception("Cannot schedule a pickup in the past.");

            // Check for conflicts
            var hasConflict = await _scheduleRepository.HasConflictAsync(
                dto.BinId, dto.ScheduledDate, dto.TimeSlot);
            if (hasConflict)
                throw new Exception(
                    $"This bin already has a scheduled pickup for {dto.ScheduledDate:yyyy-MM-dd} {dto.TimeSlot}.");

            var schedule = new CollectionSchedule
            {
                BinId = dto.BinId,
                RequestedByUserId = requestedByUserId,
                ScheduledDate = dto.ScheduledDate,
                TimeSlot = dto.TimeSlot,
                Status = "pending",
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _scheduleRepository.CreateAsync(schedule);
            var result = await _scheduleRepository.GetByIdAsync(created.Id);
            return MapToDto(result);
        }

        public async Task<ScheduleResponseDto> AssignCollectorAsync(
            int scheduleId, AssignCollectorDto dto)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
                throw new Exception($"Schedule with ID {scheduleId} not found.");

            // Make sure collector exists and has correct role
            var collector = await _userRepository.GetByIdAsync(dto.CollectorId);
            if (collector == null)
                throw new Exception($"Collector with ID {dto.CollectorId} not found.");
            if (collector.Role != "collector" && collector.Role != "admin")
                throw new Exception("This user is not a collector.");

            schedule.AssignedCollectorId = dto.CollectorId;
            schedule.Status = "accepted";
            await _scheduleRepository.UpdateAsync(schedule);

            return MapToDto(schedule);
        }

        public async Task<ScheduleResponseDto> UpdateStatusAsync(
            int scheduleId, UpdateScheduleStatusDto dto, int updatedByUserId)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
                throw new Exception($"Schedule with ID {scheduleId} not found.");

            // Only the assigned collector or admin can complete a schedule
            if (dto.Status == "completed")
            {
                if (schedule.AssignedCollectorId == null)
                    throw new Exception(
                        "Cannot complete a schedule with no assigned collector.");

                // Reset the bin after completion
                var bin = await _binRepository.GetByIdAsync(schedule.BinId);
                if (bin != null)
                {
                    bin.FillLevel = 0;
                    bin.NeedsPickup = false;
                    bin.LastUpdated = DateTime.UtcNow;
                    await _binRepository.UpdateAsync(bin);
                }

                // Award collector 15 credits for scheduled pickup
                var collector = await _userRepository
                    .GetByIdAsync(schedule.AssignedCollectorId.Value);
                if (collector != null)
                {
                    collector.CreditBalance += 15;
                    await _userRepository.UpdateAsync(collector);

                    await _creditRepository.CreateAsync(new CreditTransaction
                    {
                        UserId = collector.Id,
                        Amount = 15,
                        Reason = $"Completed scheduled pickup — Bin at {schedule.Bin?.Location}",
                        TransactedAt = DateTime.UtcNow
                    });
                }
            }

            schedule.Status = dto.Status;
            await _scheduleRepository.UpdateAsync(schedule);
            return MapToDto(schedule);
        }

        private ScheduleResponseDto MapToDto(CollectionSchedule cs) =>
            new ScheduleResponseDto
            {
                Id = cs.Id,
                BinId = cs.BinId,
                BinLocation = cs.Bin?.Location,
                RequestedByName = cs.RequestedBy?.FullName,
                AssignedCollectorName = cs.AssignedCollector?.FullName,
                ScheduledDate = cs.ScheduledDate,
                TimeSlot = cs.TimeSlot,
                Status = cs.Status,
                Notes = cs.Notes,
                CreatedAt = cs.CreatedAt
            };
    }
}
