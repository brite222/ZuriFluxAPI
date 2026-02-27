using System.ComponentModel.DataAnnotations;

namespace ZuriFluxAPI.DTOs
{
    // What the API returns when someone requests bin info
    public class BinResponseDto
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int FillLevel { get; set; }
        public string WasteType { get; set; }
        public bool NeedsPickup { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class CreateBinDto
    {
        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, MinimumLength = 3)]
        public string Location { get; set; }

        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public double Longitude { get; set; }

        [Required]
        [RegularExpression("^(organic|recyclable|general)$",
            ErrorMessage = "WasteType must be organic, recyclable, or general.")]
        public string WasteType { get; set; }
    }

    public class SensorReadingDto
    {
        [Required]
        public int BinId { get; set; }

        [Range(0, 100, ErrorMessage = "FillLevel must be between 0 and 100.")]
        public int FillLevel { get; set; }

        [Range(0, 100)]
        public double OdorLevel { get; set; }

        public string GasDetected { get; set; }
    }
}
