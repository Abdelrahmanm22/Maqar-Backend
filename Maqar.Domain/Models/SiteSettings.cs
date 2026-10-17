namespace Maqar.Domain.Models
{
    /// <summary>
    /// Site-wide contact details shown by the public site (footer, contact page) and used as the Call /
    /// WhatsApp fallback when a listing's broker has no number. A single row, Id = 1, seeded by migration.
    /// </summary>
    public class SiteSettings : BaseModel
    {
        public const int SingletonId = 1;

        /// <summary>E.164-style number for tel: links, e.g. "+201000000000".</summary>
        public string Phone { get; set; } = string.Empty;
        /// <summary>How the number is printed, e.g. "+20 100 000 0000".</summary>
        public string PhoneDisplay { get; set; } = string.Empty;
        /// <summary>Digits only, as wa.me expects, e.g. "201000000000".</summary>
        public string WhatsApp { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AddressAr { get; set; }
        public string? AddressEn { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
