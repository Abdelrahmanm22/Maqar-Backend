using Maqar.Domain.Enums;
using Maqar.Domain.Models;
using Maqar.Domain.Specifications.ContactMessages;

namespace Maqar.Domain.Services.ContactMessages
{
    /// <summary>
    /// The admin inbox. Scoped like properties (see <see cref="OwnerScope"/>): without Properties.ViewAll a
    /// user sees only inquiries about listings they created.
    /// </summary>
    public interface IContactMessageService
    {
        /// <summary>
        /// A message from the public site. When <paramref name="propertyId"/> is set it must be a published
        /// listing, otherwise this fails with "not found".
        /// </summary>
        Task<OperationResult<int>> CreateAsync(string name, string email, string phone, string message, int? propertyId);

        Task<PagedResult<ContactMessage>> GetPagedAsync(ContactMessageSpecParams specParams, string? ownerScope);

        Task<int> CountNewAsync(string? ownerScope);

        /// <summary>Null when missing or out of scope. Opening a New message marks it Read when <paramref name="markRead"/>.</summary>
        Task<ContactMessage?> GetAsync(int id, string? ownerScope, bool markRead);

        Task<OperationResult> ChangeStatusAsync(int id, string? ownerScope, ContactMessageStatus status);

        Task<OperationResult> DeleteAsync(int id, string? ownerScope);
    }
}
