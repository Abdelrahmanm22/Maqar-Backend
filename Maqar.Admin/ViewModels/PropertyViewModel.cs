using System.ComponentModel.DataAnnotations;
using Maqar.Domain.Enums;
using Maqar.Domain.Models;
using Maqar.Domain.Services;
using Maqar.Domain.Specifications.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Maqar.Admin.ViewModels
{
    public class PropertyFormViewModel : IValidatableObject
    {
        public int Id { get; set; }
        public bool IsCreate => Id == 0;

        [Required, Display(Name = "Property type")]
        public PropertyType Type { get; set; } = PropertyType.Apartment;

        [Required, Display(Name = "Listing")]
        public ListingType ListingType { get; set; } = ListingType.Sale;

        [Display(Name = "Rented as")]
        public RentalUnit? RentalUnit { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, 10_000_000_000, ErrorMessage = "Enter a price above zero.")]
        [Display(Name = "Price (EGP)")]
        public decimal? Price { get; set; }

        [Display(Name = "Price is negotiable")]
        public bool IsNegotiable { get; set; }

        [Range(0, 50)]
        public int? Bedrooms { get; set; }

        [Range(0, 50)]
        public int? Bathrooms { get; set; }

        [Required(ErrorMessage = "Size is required.")]
        [Range(1, 1_000_000, ErrorMessage = "Enter a size above zero.")]
        [Display(Name = "Size (m²)")]
        public decimal? Size { get; set; }

        public FinishingType? Finishing { get; set; }

        [Range(-5, 200)]
        public int? Floor { get; set; }

        [Required, StringLength(200), Display(Name = "Title (Arabic)")]
        public string TitleAr { get; set; } = string.Empty;

        [Required, StringLength(200), Display(Name = "Title (English)")]
        public string TitleEn { get; set; } = string.Empty;

        [Required, Display(Name = "Description (Arabic)")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required, Display(Name = "Description (English)")]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required, StringLength(250), Display(Name = "Location (Arabic)")]
        public string LocationAr { get; set; } = string.Empty;

        [Required, StringLength(250), Display(Name = "Location (English)")]
        public string LocationEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose an area."), Display(Name = "Area")]
        public int? AreaId { get; set; }

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        /// <summary>Repeated name="SelectedFeatureIds" checkboxes — never asp-for (it adds a hidden "false").</summary>
        public List<int> SelectedFeatureIds { get; set; } = new();

        /// <summary>Filled by the Dropzone through a hidden multi-file input.</summary>
        public List<IFormFile> NewImages { get; set; } = new();

        // ─── Display only ───────────────────────────────────────────────────────
        public List<PropertyImage> ExistingImages { get; set; } = new();
        public List<SelectListItem> Areas { get; set; } = new();
        public List<Feature> Features { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ListingType == ListingType.Rent && RentalUnit is null)
                yield return new ValidationResult("Choose what is rented — the whole unit, a room or a bed.", new[] { nameof(RentalUnit) });

            if ((Latitude is null) != (Longitude is null))
                yield return new ValidationResult("Enter both latitude and longitude, or neither.", new[] { nameof(Longitude) });

            if (Type != PropertyType.Land)
            {
                if (Bedrooms is null)
                    yield return new ValidationResult("Bedrooms is required.", new[] { nameof(Bedrooms) });
                if (Bathrooms is null)
                    yield return new ValidationResult("Bathrooms is required.", new[] { nameof(Bathrooms) });
            }
        }
    }

    public class PropertyListViewModel
    {
        public PropertySpecParams Filters { get; set; } = new();
        public PagedResult<Property> Page { get; set; } = new(1, 20, 0, Array.Empty<Property>());

        /// <summary>True for Properties.ViewAll — shows the "Added by" column and filter.</summary>
        public bool CanViewAll { get; set; }

        public List<SelectListItem> Areas { get; set; } = new();
        public List<SelectListItem> Brokers { get; set; } = new();
    }

    public class PropertyDetailsViewModel
    {
        public Property Property { get; set; } = null!;
        public IReadOnlyList<ContactMessage> Inquiries { get; set; } = Array.Empty<ContactMessage>();
        public bool CanViewInquiries { get; set; }
        public PropertyStatsViewModel Stats { get; set; } = null!;
    }

    /// <summary>What _Pager needs — built from any PagedResult via <see cref="From{T}"/>.</summary>
    public sealed record PagerViewModel(int PageIndex, int PageSize, int Count, int TotalPages)
    {
        public static PagerViewModel From<T>(PagedResult<T> page) =>
            new(page.PageIndex, page.PageSize, page.Count, page.TotalPages);
    }

    /// <summary>The Edit page's existing-images strip — also returned alone by Property/Images after an AJAX change.</summary>
    public class PropertyImageStripViewModel
    {
        public int PropertyId { get; set; }
        public List<PropertyImage> Images { get; set; } = new();
    }

    public class ChangePropertyStatusRequest
    {
        public int Id { get; set; }
        public PropertyStatus Status { get; set; }
    }
}
