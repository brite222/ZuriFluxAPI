namespace ZuriFluxAPI.Models
{
    public class WasteCollection
    {
        public int Id { get; set; }
        public int BinId { get; set; }
        public int CollectorId { get; set; }
        public DateTime CollectedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public Bin Bin { get; set; } = null!;
        public User Collector { get; set; } = null!;
    }
}