using Maqar.Domain.Enums;

namespace Maqar.Domain.Models
{
    /// <summary>
    /// One tracked interaction (view, WhatsApp tap, call tap). Written by the public API, de-duplicated per
    /// visitor per listing per type over a short window, and aggregated for the dashboard.
    /// </summary>
    public class PropertyEvent : BaseModel
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;

        public PropertyEventType Type { get; set; }

        /// <summary>SHA-256 (hex) of IP + User-Agent + a server salt — identifies a repeat visitor without storing the IP.</summary>
        public string VisitorHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
