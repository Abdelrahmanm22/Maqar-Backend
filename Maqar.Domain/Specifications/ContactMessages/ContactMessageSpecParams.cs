using Maqar.Domain.Enums;

namespace Maqar.Domain.Specifications.ContactMessages
{
    public class ContactMessageSpecParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 20;
        private int _pageIndex = 1;
        private string? _search;

        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 20 : Math.Min(value, MaxPageSize);
        }

        /// <summary>Matches name, email, phone or message text.</summary>
        public string? Search
        {
            get => _search;
            set => _search = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public ContactMessageStatus? Status { get; set; }
    }
}
