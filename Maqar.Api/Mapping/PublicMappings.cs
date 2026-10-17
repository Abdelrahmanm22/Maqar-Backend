using Maqar.Api.Dtos;
using Maqar.Api.Helpers;
using Maqar.Domain.Models;
using Maqar.Domain.Services;
using Maqar.Domain.Services.Analytics;

namespace Maqar.Api.Mapping
{
    /// <summary>
    /// Entity → public DTO. Explicit rather than AutoMapper: the contract is the frontend's types, and every
    /// rule (absolute image URLs, cover first, contact fallback, date format) is visible in one place.
    /// </summary>
    public static class PublicMappings
    {
        public static PropertyDto ToDto(this Property p, string mediaBaseUrl, SiteSettings site) => new(
            Id: p.Id,
            Type: p.Type,
            ListingType: p.ListingType,
            RentalUnit: p.RentalUnit,
            Area: p.Area.Key,
            Featured: p.IsFeatured,
            Price: p.Price,
            IsNegotiable: p.IsNegotiable,
            Bedrooms: p.Bedrooms,
            Bathrooms: p.Bathrooms,
            Size: p.Size,
            Finishing: p.Finishing,
            Floor: p.Floor,
            CreatedAt: p.CreatedAt.ToString("yyyy-MM-dd"),
            Images: p.Images
                .OrderByDescending(i => i.IsCover).ThenBy(i => i.DisplayOrder)
                .Select(i => AbsoluteUrl(mediaBaseUrl, i.ImageUrl))
                .ToList(),
            Features: p.Features.OrderBy(f => f.DisplayOrder).Select(f => f.Key).ToList(),
            Title: new LocalizedDto(p.TitleAr, p.TitleEn),
            Location: new LocalizedDto(p.LocationAr, p.LocationEn),
            Description: new LocalizedDto(p.DescriptionAr, p.DescriptionEn),
            Coordinates: p.Latitude is { } lat && p.Longitude is { } lng ? new CoordinatesDto(lat, lng) : null,
            Contact: ContactFor(p, site));

        public static IReadOnlyList<PropertyDto> ToDtos(this IEnumerable<Property> properties, string mediaBaseUrl, SiteSettings site) =>
            properties.Select(p => p.ToDto(mediaBaseUrl, site)).ToList();

        public static PagedResponse<PropertyDto> ToDto(this PagedResult<Property> page, string mediaBaseUrl, SiteSettings site) =>
            new(page.PageIndex, page.PageSize, page.Count, page.TotalPages, page.Data.ToDtos(mediaBaseUrl, site));

        public static LookupDto ToLookupDto(this Area a) => new(a.Key, new LocalizedDto(a.NameAr, a.NameEn), null);

        public static LookupDto ToLookupDto(this Feature f) => new(f.Key, new LocalizedDto(f.NameAr, f.NameEn), f.Icon);

        public static SiteSettingsDto ToDto(this SiteSettings s) => new(
            s.Phone,
            s.PhoneDisplay,
            s.WhatsApp,
            s.Email,
            s.AddressAr is null && s.AddressEn is null ? null : new LocalizedDto(s.AddressAr ?? string.Empty, s.AddressEn ?? string.Empty),
            s.FacebookUrl,
            s.InstagramUrl);

        public static RangeDto? ToDto(this ValueRange? range) => range is null ? null : new RangeDto(range.Min, range.Max);

        /// <summary>
        /// Leads go to the broker who listed it: their phone for Call, their WhatsApp number (else their phone)
        /// for WhatsApp. An inactive or deleted broker, or a missing number, falls back to the site numbers.
        /// </summary>
        private static ContactDto ContactFor(Property p, SiteSettings site)
        {
            var broker = p.CreatedBy is { IsActive: true } user ? user : null;

            var phone = string.IsNullOrWhiteSpace(broker?.PhoneNumber) ? site.Phone : broker.PhoneNumber!;
            var whatsapp = PhoneNumbers.ToWhatsApp(broker?.WhatsAppNumber)
                           ?? PhoneNumbers.ToWhatsApp(broker?.PhoneNumber)
                           ?? site.WhatsApp;

            return new ContactDto(phone, whatsapp);
        }

        // Not Uri.TryCreate(..., Absolute): on Linux "/Files/x.jpg" parses as an absolute file:// URI.
        private static string AbsoluteUrl(string baseUrl, string path) =>
            path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? path
                : baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
    }
}
