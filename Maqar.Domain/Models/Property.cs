using Maqar.Domain.Enums;
using Maqar.Domain.Models.Identity;

namespace Maqar.Domain.Models
{
    public class Property : BaseModel, IAuditableEntity, ISoftDeletable
    {
        #region Classification
        public PropertyType Type { get; set; }
        public ListingType ListingType { get; set; }
        /// <summary>Only for rentals — a DB check constraint rejects it on a sale listing.</summary>
        public RentalUnit? RentalUnit { get; set; }
        public PropertyStatus Status { get; set; } = PropertyStatus.Draft;
        public bool IsFeatured { get; set; }
        #endregion

        #region Price (EGP)
        /// <summary>The total for a sale listing, the monthly amount for a rental.</summary>
        public decimal Price { get; set; }
        public bool IsNegotiable { get; set; }
        #endregion

        #region Specs
        // Nullable because they don't apply to every type (land has no bedrooms or finishing).
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        /// <summary>Square metres.</summary>
        public decimal Size { get; set; }
        public FinishingType? Finishing { get; set; }
        public int? Floor { get; set; }
        #endregion

        #region Text (Arabic / English)
        public string TitleAr { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        /// <summary>Street / compound line shown under the title — the Area holds the district.</summary>
        public string LocationAr { get; set; } = string.Empty;
        public string LocationEn { get; set; } = string.Empty;
        #endregion

        #region Location
        public int AreaId { get; set; }
        public Area Area { get; set; } = null!;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        #endregion

        #region Audit
        /// <summary>Set by the creating service from ICurrentUserService; null if that user is deleted.</summary>
        public string? CreatedById { get; set; }
        public AppUser? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        #endregion

        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public ICollection<Feature> Features { get; set; } = new List<Feature>();
        public ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();
    }
}
