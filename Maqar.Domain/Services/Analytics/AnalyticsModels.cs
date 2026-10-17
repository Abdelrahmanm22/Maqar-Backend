using Maqar.Domain.Enums;

namespace Maqar.Domain.Services.Analytics
{
    public sealed record ValueRange(decimal Min, decimal Max);

    /// <summary>Real min/max over published listings — what the public filter sliders are bounded by. Null when there are none.</summary>
    public sealed record PropertyBounds(ValueRange? SalePrice, ValueRange? RentPrice, ValueRange? Size);

    public sealed record EventTotals(int Views, int WhatsAppClicks, int CallClicks)
    {
        public int Contacts => WhatsAppClicks + CallClicks;
    }

    public sealed record TopProperty(int PropertyId, string TitleEn, string? CoverUrl, int Count);

    public sealed record TopPropertiesQuery(DateTime Since, string? OwnerScope, int Take, params PropertyEventType[] Types);
}
