namespace ZuriFluxAPI.DTOs
{
    // What a collector sends when logging a pickup
    public class CreateCollectionDto
    {
        public int BinId { get; set; }
        public int CollectorId { get; set; }
        public string Notes { get; set; }  // optional remarks
    }

    // What the API returns
    public class CollectionResponseDto
    {
        public int Id { get; set; }
        public int BinId { get; set; }
        public string BinLocation { get; set; }    // pulled from Bin
        public string CollectorName { get; set; }  // pulled from User
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CollectedAt { get; set; }
    }

    // For updating status (e.g. mark as failed)
    public class UpdateCollectionStatusDto
    {
        public string Status { get; set; }  // "completed", "failed"
    }
}
