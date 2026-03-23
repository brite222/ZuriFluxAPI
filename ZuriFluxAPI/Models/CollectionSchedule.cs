namespace ZuriFluxAPI.Models
{
    public class CollectionSchedule
    {
        public int Id { get; set; }
        public int BinId { get; set; }
        public int RequestedByUserId { get; set; }      // citizen who booked
        public int? AssignedCollectorId { get; set; }   // collector assigned to it
        public DateTime ScheduledDate { get; set; }     // requested date
        public string TimeSlot { get; set; }            // "morning", "afternoon", "evening"
        public string Status { get; set; }              // "pending", "accepted", "completed", "cancelled"
        public string Notes { get; set; }               // any special instructions
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Bin Bin { get; set; }
        public User RequestedBy { get; set; }
        public User AssignedCollector { get; set; }
    }
}
