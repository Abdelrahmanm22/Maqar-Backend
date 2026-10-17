using Maqar.Domain.Services.Analytics;

namespace Maqar.Domain.Repositories
{
    /// <summary>
    /// Read-only aggregations the generic repository can't express (Min/Max, GROUP BY + COUNT). Kept to
    /// projections that never load whole entities. Honours the soft-delete filter and the ownership scope
    /// (<c>ownerScope</c> null = every listing, otherwise only the listings that user created).
    /// </summary>
    public interface IPropertyStatsRepository
    {
        Task<PropertyBounds> GetPublishedBoundsAsync();

        /// <summary>Totals since <paramref name="since"/> (null = all time), across the scope or for one listing.</summary>
        Task<EventTotals> GetEventTotalsAsync(DateTime? since, string? ownerScope, int? propertyId = null);

        /// <summary>The listings with the most events of the given types since <c>Since</c>.</summary>
        Task<IReadOnlyList<TopProperty>> GetTopAsync(TopPropertiesQuery query);
    }
}
