using System.Linq.Expressions;
using Maqar.Domain.Enums;
using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.Properties
{
    /// <summary>
    /// The public site sees <b>Published</b> listings only (soft-deleted ones are already hidden by the
    /// query filter). Every public spec starts from <see cref="IsPublic"/>, so a draft can never leak.
    /// </summary>
    public static class PublicPropertyCriteria
    {
        public static readonly Expression<Func<Property, bool>> IsPublic = p => p.Status == PropertyStatus.Published;

        public static Expression<Func<Property, bool>> Build(PublicPropertySpecParams q)
        {
            var area = q.Area;

            return p =>
                p.Status == PropertyStatus.Published &&
                (area == null || p.Area.Key == area) &&
                (!q.Type.HasValue || p.Type == q.Type) &&
                (!q.ListingType.HasValue || p.ListingType == q.ListingType) &&
                (!q.RentalUnit.HasValue || p.RentalUnit == q.RentalUnit) &&
                (!q.MinPrice.HasValue || p.Price >= q.MinPrice) &&
                (!q.MaxPrice.HasValue || p.Price <= q.MaxPrice) &&
                (!q.MinSize.HasValue || p.Size >= q.MinSize) &&
                (!q.MaxSize.HasValue || p.Size <= q.MaxSize) &&
                (!q.Bedrooms.HasValue || p.Bedrooms >= q.Bedrooms);
        }

        /// <summary>What a public listing response needs: area key, images, feature keys, the broker's numbers.</summary>
        internal static void IncludeCardData(BaseSpecifications<Property> spec)
        {
            spec.Includes.Add(p => p.Area);
            spec.Includes.Add(p => p.Images);
            spec.Includes.Add(p => p.Features);
            spec.Includes.Add(p => p.CreatedBy!);
            spec.AsSplitQuery = true; // Images + Features are two collections
        }
    }

    public class PublicPropertyWithFiltersSpec : BaseSpecifications<Property>
    {
        public PublicPropertyWithFiltersSpec(PublicPropertySpecParams q) : base(PublicPropertyCriteria.Build(q))
        {
            PublicPropertyCriteria.IncludeCardData(this);

            switch (q.Sort)
            {
                case "asc": SetOrderBy(p => p.Price); break;
                case "desc": SetOrderByDesc(p => p.Price); break;
                default: SetOrderByDesc(p => p.CreatedAt); break;
            }

            ApplyPagination((q.Page - 1) * q.PageSize, q.PageSize);
        }
    }

    public class PublicPropertyCountSpec : BaseSpecifications<Property>
    {
        public PublicPropertyCountSpec(PublicPropertySpecParams q) : base(PublicPropertyCriteria.Build(q))
        {
        }
    }

    public class PublicPropertyByIdSpec : BaseSpecifications<Property>
    {
        public PublicPropertyByIdSpec(int id) : base(p => p.Id == id && p.Status == PropertyStatus.Published)
        {
            PublicPropertyCriteria.IncludeCardData(this);
        }
    }

    /// <summary>Existence check only — no includes.</summary>
    public class PublicPropertyCountByIdSpec : BaseSpecifications<Property>
    {
        public PublicPropertyCountByIdSpec(int id) : base(p => p.Id == id && p.Status == PropertyStatus.Published)
        {
        }
    }

    /// <summary>Published + featured, newest first — the home page's "Featured properties".</summary>
    public class FeaturedPropertiesSpec : BaseSpecifications<Property>
    {
        public FeaturedPropertiesSpec(int take) : base(p => p.Status == PropertyStatus.Published && p.IsFeatured)
        {
            PublicPropertyCriteria.IncludeCardData(this);
            SetOrderByDesc(p => p.CreatedAt);
            ApplyPagination(0, take);
        }
    }

    /// <summary>"You may also like": same listing type, and the same area or the same property type.</summary>
    public class SimilarPropertiesSpec : BaseSpecifications<Property>
    {
        public SimilarPropertiesSpec(Property source, int take)
            : base(p => p.Status == PropertyStatus.Published
                        && p.Id != source.Id
                        && p.ListingType == source.ListingType
                        && (p.AreaId == source.AreaId || p.Type == source.Type))
        {
            PublicPropertyCriteria.IncludeCardData(this);
            SetOrderByDesc(p => p.CreatedAt);
            ApplyPagination(0, take);
        }
    }

    /// <summary>Dedup probe for tracking: the same visitor did the same thing to the same listing recently.</summary>
    public class RecentEventSpec : BaseSpecifications<PropertyEvent>
    {
        public RecentEventSpec(int propertyId, PropertyEventType type, string visitorHash, DateTime since)
            : base(e => e.PropertyId == propertyId && e.Type == type && e.VisitorHash == visitorHash && e.CreatedAt >= since)
        {
        }
    }
}
