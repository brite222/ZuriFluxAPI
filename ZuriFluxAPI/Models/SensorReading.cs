namespace ZuriFluxAPI.Models
{
    public class SensorReading
    {
        public int Id { get; set; }
        public int BinId { get; set; }              // which bin sent this
        public int FillLevel { get; set; }
        public double OdorLevel { get; set; }
        public string GasDetected { get; set; }
        public DateTime RecordedAt { get; set; }

        // Navigation - links back to Bin
        public Bin Bin { get; set; }
    }
}
