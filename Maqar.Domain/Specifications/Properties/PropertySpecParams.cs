using Maqar.Domain.Enums;

namespace Maqar.Domain.Specifications.Properties
{
    /// <summary>Filter / sort / paging input for the property list. Bound straight from the query string.</summary>
    public class PropertySpecParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 20;
        private int _pageIndex = 1;
        private string? _search;

        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 20 : Math.Min(value, MaxPageSize);
        }

        /// <summary>Matches the Arabic or English title or location.</summary>
        public string? Search
        {
            get => _search;
            set => _search = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public PropertyStatus? Status { get; set; }
        public PropertyType? Type { get; set; }
        public ListingType? ListingType { get; set; }
        public int? AreaId { get; set; }
        public bool? IsFeatured { get; set; }

        /// <summary>"Added by" filter. Only honoured for unscoped (ViewAll) users — a scoped user is already pinned to themselves.</summary>
        public string? CreatedById { get; set; }

        /// <summary>newest (default) | oldest | priceAsc | priceDesc</summary>
        public string? Sort { get; set; }

        public bool HasFilters =>
            Search is not null || Status.HasValue || Type.HasValue || ListingType.HasValue
            || AreaId.HasValue || IsFeatured.HasValue || !string.IsNullOrEmpty(CreatedById);
    }
}
