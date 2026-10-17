using Maqar.Domain.Enums;

namespace Maqar.Domain.Specifications.Properties
{
    /// <summary>
    /// The public site's listing query. Property names match the frontend's URL search params
    /// (src/routes/properties.index.tsx) so it can pass them straight through: area, type, listingType,
    /// rentalUnit, minPrice, maxPrice, minSize, maxSize, bedrooms, sort, page, pageSize.
    /// </summary>
    public class PublicPropertySpecParams
    {
        public const int DefaultPageSize = 6;
        public const int MaxPageSize = 24;

        private int _page = 1;
        private int _pageSize = DefaultPageSize;
        private string? _area;

        /// <summary>Area key, e.g. "fifthSettlement".</summary>
        public string? Area
        {
            get => _area;
            set => _area = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public PropertyType? Type { get; set; }
        public ListingType? ListingType { get; set; }
        public RentalUnit? RentalUnit { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinSize { get; set; }
        public decimal? MaxSize { get; set; }

        /// <summary>At least this many bedrooms. Listings without bedrooms (land) are excluded when set.</summary>
        public int? Bedrooms { get; set; }

        /// <summary>newest (default) | asc (price low → high) | desc (price high → low).</summary>
        public string? Sort { get; set; }

        /// <summary>1-based.</summary>
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? DefaultPageSize : Math.Min(value, MaxPageSize);
        }
    }
}
