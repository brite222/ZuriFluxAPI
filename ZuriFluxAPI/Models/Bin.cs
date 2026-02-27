namespace ZuriFluxAPI.Models
{
    public class Bin
    {
        public int Id { get; set; }
        public string Location { get; set; }        // e.g. "Lagos Island Market"
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int FillLevel { get; set; }          // 0-100 percentage
        public string WasteType { get; set; }       // "organic", "recyclable", "general"
        public bool NeedsPickup { get; set; }
        public DateTime LastUpdated { get; set; }

        // Navigation property - one bin has many readings
        public ICollection<SensorReading> SensorReadings { get; set; }
    }
}
