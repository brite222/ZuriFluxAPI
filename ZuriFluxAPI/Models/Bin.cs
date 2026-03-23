namespace ZuriFluxAPI.Models
{
    public class Bin
    {
        public int Id { get; set; }
        public string Location { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int FillLevel { get; set; }
        public string WasteType { get; set; } = string.Empty;
        public bool NeedsPickup { get; set; }
        public DateTime LastUpdated { get; set; }
        public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    }
}