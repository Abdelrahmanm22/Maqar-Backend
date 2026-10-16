using System.Linq.Expressions;
using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.Properties
{
    /// <summary>
    /// The one WHERE clause shared by <see cref="PropertyWithFiltersSpec"/> and <see cref="PropertyCountSpec"/>,
    /// so the page and the total behind the pager can never disagree. The ownership scope is part of it —
    /// see <c>OwnerScope</c>.
    /// </summary>
    public static class PropertyCriteria
    {
        public static Expression<Func<Property, bool>> Build(PropertySpecParams p, string? ownerScope)
        {
            // A scoped user is pinned to their own listings; the "Added by" filter is for ViewAll users only.
            var createdBy = ownerScope ?? (string.IsNullOrEmpty(p.CreatedById) ? null : p.CreatedById);
            var search = p.Search;

            return x =>
                (createdBy == null || x.CreatedById == createdBy) &&
                (!p.Status.HasValue || x.Status == p.Status) &&
                (!p.Type.HasValue || x.Type == p.Type) &&
                (!p.ListingType.HasValue || x.ListingType == p.ListingType) &&
                (!p.AreaId.HasValue || x.AreaId == p.AreaId) &&
                (!p.IsFeatured.HasValue || x.IsFeatured == p.IsFeatured) &&
                (search == null
                    || x.TitleEn.Contains(search) || x.TitleAr.Contains(search)
                    || x.LocationEn.Contains(search) || x.LocationAr.Contains(search));
        }
    }
}
