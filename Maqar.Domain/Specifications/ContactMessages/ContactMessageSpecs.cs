using Maqar.Domain.Enums;
using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.ContactMessages
{
    /// <summary>One inbox page, newest first.</summary>
    public class ContactMessageWithFiltersSpec : BaseSpecifications<ContactMessage>
    {
        public ContactMessageWithFiltersSpec(ContactMessageSpecParams p, string? ownerScope)
            : base(ContactMessageCriteria.Build(p, ownerScope))
        {
            Includes.Add(m => m.Property!);
            SetOrderByDesc(m => m.CreatedAt);
            ApplyPagination((p.PageIndex - 1) * p.PageSize, p.PageSize);
        }
    }

    public class ContactMessageCountSpec : BaseSpecifications<ContactMessage>
    {
        public ContactMessageCountSpec(ContactMessageSpecParams p, string? ownerScope)
            : base(ContactMessageCriteria.Build(p, ownerScope))
        {
        }
    }

    /// <summary>Unread badge in the sidebar / inbox tabs, honouring the scope.</summary>
    public class ContactMessageNewCountSpec : BaseSpecifications<ContactMessage>
    {
        public ContactMessageNewCountSpec(string? ownerScope)
            : base(ContactMessageCriteria.Build(new ContactMessageSpecParams { Status = ContactMessageStatus.New }, ownerScope))
        {
        }
    }

    public class ContactMessageWithPropertySpec : BaseSpecifications<ContactMessage>
    {
        public ContactMessageWithPropertySpec(int id, string? ownerScope)
            : base(ContactMessageCriteria.ById(id, ownerScope))
        {
            Includes.Add(m => m.Property!);
        }
    }

    /// <summary>Inquiries shown on a property's details page — the property itself is already scope-checked.</summary>
    public class ContactMessagesForPropertySpec : BaseSpecifications<ContactMessage>
    {
        public ContactMessagesForPropertySpec(int propertyId) : base(m => m.PropertyId == propertyId)
        {
            SetOrderByDesc(m => m.CreatedAt);
        }
    }
}
