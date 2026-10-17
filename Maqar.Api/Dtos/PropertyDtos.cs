using Maqar.Domain.Enums;

namespace Maqar.Api.Dtos
{
    /// <summary>Bilingual text, exactly the frontend's <c>Localized</c> type.</summary>
    public sealed record LocalizedDto(string Ar, string En);

    public sealed record CoordinatesDto(decimal Lat, decimal Lng);

    /// <summary>Who the Call / WhatsApp buttons reach: the listing's broker, falling back to the site numbers.</summary>
    public sealed record ContactDto(string Phone, string Whatsapp);

    /// <summary>
    /// A listing, shaped like the frontend's <c>Property</c> type (src/data/properties.ts). Enums serialize as
    /// camelCase keys ("apartment", "rent", "superLux"), <c>area</c> and <c>features</c> are lookup keys,
    /// and every text field is <see cref="LocalizedDto"/>.
    /// </summary>
    public sealed record PropertyDto(
        int Id,
        PropertyType Type,
        ListingType ListingType,
        RentalUnit? RentalUnit,
        string Area,
        bool Featured,
        decimal Price,
        bool IsNegotiable,
        int? Bedrooms,
        int? Bathrooms,
        decimal Size,
        FinishingType? Finishing,
        int? Floor,
        string CreatedAt,
        IReadOnlyList<string> Images,
        IReadOnlyList<string> Features,
        LocalizedDto Title,
        LocalizedDto Location,
        LocalizedDto Description,
        CoordinatesDto? Coordinates,
        ContactDto Contact);

    /// <summary>One page of results. <c>pageIndex</c> is 1-based.</summary>
    public sealed record PagedResponse<T>(int PageIndex, int PageSize, int Count, int TotalPages, IReadOnlyList<T> Data);

    public sealed record LookupDto(string Key, LocalizedDto Label, string? Icon);

    public sealed record RangeDto(decimal Min, decimal Max);

    public sealed record PriceBoundsDto(RangeDto? Sale, RangeDto? Rent);

    /// <summary>Everything the listing filters need: the option lists and the real bounds of published listings.</summary>
    public sealed record FilterMetadataDto(
        IReadOnlyList<LookupDto> Areas,
        IReadOnlyList<LookupDto> Features,
        IReadOnlyList<PropertyType> PropertyTypes,
        IReadOnlyList<ListingType> ListingTypes,
        IReadOnlyList<RentalUnit> RentalUnits,
        IReadOnlyList<FinishingType> Finishings,
        PriceBoundsDto Price,
        RangeDto? Size);
}
