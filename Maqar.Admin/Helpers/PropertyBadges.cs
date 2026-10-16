using System.Globalization;
using Maqar.Domain.Enums;

namespace Maqar.Admin.Helpers
{
    /// <summary>How a label looks and reads: text, badge classes and icon in one place.</summary>
    public sealed record Badge(string Label, string CssClass, string Icon);

    /// <summary>
    /// The single source of truth for how the listing enums read on screen, so the same value is never
    /// spelled two ways across views. The <c>bg-*-subtle</c> classes come from app-extras.css (the Skote
    /// build is Bootstrap 5.0.0-beta2); every icon is verified present in icons.min.css.
    /// </summary>
    public static class PropertyBadges
    {
        public const string Currency = "EGP";

        public static Badge ForStatus(PropertyStatus status) => status switch
        {
            PropertyStatus.Published => new("Published", "bg-success-subtle text-success", "bx-check-circle"),
            PropertyStatus.Sold => new("Sold", "bg-info-subtle text-info", "bx-purchase-tag"),
            PropertyStatus.Rented => new("Rented", "bg-primary-subtle text-primary", "bx-key"),
            PropertyStatus.Archived => new("Archived", "bg-dark-subtle text-dark", "bx-archive"),
            _ => new("Draft", "bg-secondary-subtle text-secondary", "bx-time-five"),
        };

        public static Badge ForMessage(ContactMessageStatus status) => status switch
        {
            ContactMessageStatus.Read => new("Read", "bg-info-subtle text-info", "bx-show"),
            ContactMessageStatus.Replied => new("Replied", "bg-success-subtle text-success", "bx-reply"),
            ContactMessageStatus.Archived => new("Archived", "bg-dark-subtle text-dark", "bx-archive"),
            _ => new("New", "bg-warning-subtle text-warning", "bx-envelope"),
        };

        public static string TypeLabel(PropertyType type) => type switch
        {
            PropertyType.Apartment => "Apartment",
            PropertyType.Villa => "Villa",
            PropertyType.Shop => "Shop",
            PropertyType.Land => "Land",
            PropertyType.Building => "Building",
            _ => type.ToString(),
        };

        public static string TypeIcon(PropertyType type) => type switch
        {
            PropertyType.Villa => "bx-home",
            PropertyType.Shop => "bx-store",
            PropertyType.Land => "bx-landscape",
            PropertyType.Building => "bx-buildings",
            _ => "bx-building",
        };

        public static string ListingLabel(ListingType type) => type == ListingType.Rent ? "For rent" : "For sale";

        public static string ListingCss(ListingType type) =>
            type == ListingType.Rent ? "bg-primary-subtle text-primary" : "bg-warning-subtle text-warning";

        public static string RentalUnitLabel(RentalUnit? unit) => unit switch
        {
            RentalUnit.Whole => "Whole unit",
            RentalUnit.Room => "Room",
            RentalUnit.Bed => "Bed",
            _ => "—",
        };

        public static string FinishingLabel(FinishingType? finishing) => finishing switch
        {
            FinishingType.SuperLux => "Super lux",
            FinishingType.Lux => "Lux",
            FinishingType.SemiFinished => "Semi-finished",
            FinishingType.Core => "Core & shell",
            _ => "—",
        };

        /// <summary>"EGP 4,750,000", plus "/ month" for rentals — invariant culture so separators never flip.</summary>
        public static string Price(decimal price, ListingType listingType) =>
            $"{Currency} {price.ToString("#,##0.##", CultureInfo.InvariantCulture)}"
            + (listingType == ListingType.Rent ? " / month" : string.Empty);
    }
}
