using Maqar.Domain.Enums;
using Maqar.Domain.Models;
using Maqar.Domain.Services;
using Maqar.Domain.Specifications.ContactMessages;

namespace Maqar.Admin.ViewModels
{
    public class ContactMessageListViewModel
    {
        public ContactMessageSpecParams Filters { get; set; } = new();
        public PagedResult<ContactMessage> Page { get; set; } = new(1, 20, 0, Array.Empty<ContactMessage>());
        public int NewCount { get; set; }
        public bool CanViewAll { get; set; }
    }

    public class ChangeMessageStatusRequest
    {
        public int Id { get; set; }
        public ContactMessageStatus Status { get; set; }
    }
}
