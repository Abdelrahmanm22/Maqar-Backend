using System.Linq.Expressions;
using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.ContactMessages
{
    /// <summary>
    /// Shared WHERE for the inbox list and its count. A scoped user (no Properties.ViewAll) sees only
    /// inquiries about listings they created — general contact-form messages need ViewAll.
    /// </summary>
    public static class ContactMessageCriteria
    {
        public static Expression<Func<ContactMessage, bool>> Build(ContactMessageSpecParams p, string? ownerScope)
        {
            var search = p.Search;

            return m =>
                (ownerScope == null || (m.PropertyId != null && m.Property!.CreatedById == ownerScope)) &&
                (!p.Status.HasValue || m.Status == p.Status) &&
                (search == null
                    || m.Name.Contains(search) || m.Email.Contains(search)
                    || m.Phone.Contains(search) || m.Message.Contains(search));
        }

        /// <summary>One message, inside the scope or not at all.</summary>
        public static Expression<Func<ContactMessage, bool>> ById(int id, string? ownerScope) =>
            m => m.Id == id && (ownerScope == null || (m.PropertyId != null && m.Property!.CreatedById == ownerScope));
    }
}
