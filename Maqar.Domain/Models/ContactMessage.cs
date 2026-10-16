using Maqar.Domain.Enums;

namespace Maqar.Domain.Models
{
    /// <summary>A message from the public contact form, or an inquiry sent from a listing page.</summary>
    public class ContactMessage : BaseModel, IAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ContactMessageStatus Status { get; set; } = ContactMessageStatus.New;

        /// <summary>The listing the inquiry came from; null for the general contact form.</summary>
        public int? PropertyId { get; set; }
        public Property? Property { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
