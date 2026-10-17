using Maqar.Domain.Enums;

namespace Maqar.Domain.Services.Public
{
    public interface IPropertyEventService
    {
        /// <summary>The window in which a repeat of the same event by the same visitor isn't counted again.</summary>
        static readonly TimeSpan DedupWindow = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Records a view / WhatsApp tap / call tap on a published listing. A repeat within
        /// <see cref="DedupWindow"/> succeeds without writing, so callers can't tell (and needn't care).
        /// Fails only when the listing isn't public.
        /// </summary>
        Task<OperationResult> RecordAsync(int propertyId, PropertyEventType type, string visitorHash);
    }
}
