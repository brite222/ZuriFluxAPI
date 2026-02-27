namespace ZuriFluxAPI.DTOs
{
    // What the client sends as query parameters
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    // Bin specific filters + pagination
    public class BinFilterParams : PaginationParams
    {
        public string? Location { get; set; }       // filter by location name
        public string? WasteType { get; set; }      // "organic", "recyclable", "general"
        public bool? NeedsPickup { get; set; }      // true = only flagged bins
        public int? MinFillLevel { get; set; }      // e.g. 50 = bins 50%+ full
    }

    // Collection specific filters + pagination
    public class CollectionFilterParams : PaginationParams
    {
        public string? Status { get; set; }         // "completed", "failed", "pending"
        public DateTime? FromDate { get; set; }     // collections after this date
        public DateTime? ToDate { get; set; }       // collections before this date
        public int? CollectorId { get; set; }       // filter by collector
    }

    // Credit specific filters + pagination
    public class CreditFilterParams : PaginationParams
    {
        public int? UserId { get; set; }            // filter by user
        public string? Type { get; set; }           // "earned" or "deducted"
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    // Wrapper that returns paged results WITH metadata
    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
