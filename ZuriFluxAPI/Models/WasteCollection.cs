namespace ZuriFluxAPI.Models
{
    public class WasteCollection
    {
        public int Id { get; set; }
        public int BinId { get; set; }
        public int CollectorId { get; set; }        // the truck/worker who did it
        public DateTime CollectedAt { get; set; }
        public string Status { get; set; }          // "completed", "pending", "failed"
        public string Notes { get; set; }
        public Bin Bin { get; set; }
        public User Collector { get; set; }
    }
}
