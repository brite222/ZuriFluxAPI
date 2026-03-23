namespace ZuriFluxAPI.Models
{
    public class SensorReading
    {
        public int Id { get; set; }
        public int BinId { get; set; }
        public int FillLevel { get; set; }
        public double OdorLevel { get; set; }
        public string? GasDetected { get; set; }
        public DateTime RecordedAt { get; set; }
        public Bin Bin { get; set; } = null!;
    }
}