using System.ComponentModel.DataAnnotations;

namespace ZuriFluxAPI.DTOs
{
    // What a citizen sends when booking a pickup
    public class CreateScheduleDto
    {
        [Required(ErrorMessage = "BinId is required.")]
        public int BinId { get; set; }

        [Required(ErrorMessage = "Scheduled date is required.")]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [RegularExpression("^(morning|afternoon|evening)$",
            ErrorMessage = "TimeSlot must be morning, afternoon, or evening.")]
        public string TimeSlot { get; set; }

        public string Notes { get; set; }
    }

    // What admin sends when assigning a collector
    public class AssignCollectorDto
    {
        [Required]
        public int CollectorId { get; set; }
    }

    // What admin/collector sends to update status
    public class UpdateScheduleStatusDto
    {
        [Required]
        [RegularExpression("^(accepted|completed|cancelled)$",
            ErrorMessage = "Status must be accepted, completed, or cancelled.")]
        public string Status { get; set; }
    }

    // What the API returns
    public class ScheduleResponseDto
    {
        public int Id { get; set; }
        public int BinId { get; set; }
        public string BinLocation { get; set; }
        public string RequestedByName { get; set; }
        public string AssignedCollectorName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string TimeSlot { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Filter params for getting schedules
    public class ScheduleFilterParams : PaginationParams
    {
        public string Status { get; set; }
        public string TimeSlot { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CollectorId { get; set; }
        public int? UserId { get; set; }
    }
}
