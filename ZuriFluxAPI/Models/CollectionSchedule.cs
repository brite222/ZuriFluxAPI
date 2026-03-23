namespace ZuriFluxAPI.Models
{
    public class CollectionSchedule
    {
        public int Id { get; set; }
        public int BinId { get; set; }
        public int RequestedByUserId { get; set; }
        public int? AssignedCollectorId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public Bin Bin { get; set; } = null!;
        public User RequestedBy { get; set; } = null!;
        public User? AssignedCollector { get; set; }
    }
}