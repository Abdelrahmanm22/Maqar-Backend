namespace Maqar.Domain.Services.Analytics
{
    /// <summary>Dashboard numbers. Scoped like everything property-related: <c>ownerScope</c> null = all listings.</summary>
    public interface IPropertyAnalyticsService
    {
        Task<EventTotals> GetTotalsAsync(int days, string? ownerScope);

        Task<IReadOnlyList<TopProperty>> GetMostViewedAsync(int days, string? ownerScope, int take);

        /// <summary>Most WhatsApp + call taps — the listings generating leads.</summary>
        Task<IReadOnlyList<TopProperty>> GetMostContactedAsync(int days, string? ownerScope, int take);

        /// <summary>One listing (already scope-checked by the caller): last <paramref name="days"/> and all time.</summary>
        Task<(EventTotals Recent, EventTotals AllTime)> GetPropertyTotalsAsync(int propertyId, int days);
    }
}
