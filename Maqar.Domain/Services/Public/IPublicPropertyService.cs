using Maqar.Domain.Models;
using Maqar.Domain.Services.Analytics;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Domain.Services.Public
{
    /// <summary>
    /// The public site's read side. Published listings only — a draft, sold, archived or deleted listing is
    /// indistinguishable from a missing one (null). Returned entities carry Area, Images, Features and CreatedBy.
    /// </summary>
    public interface IPublicPropertyService
    {
        Task<PagedResult<Property>> GetPagedAsync(PublicPropertySpecParams query);

        Task<Property?> GetAsync(int id);

        Task<IReadOnlyList<Property>> GetFeaturedAsync(int take);

        /// <summary>Null when the source listing isn't public.</summary>
        Task<IReadOnlyList<Property>?> GetSimilarAsync(int id, int take);

        Task<PropertyBounds> GetBoundsAsync();
    }
}
