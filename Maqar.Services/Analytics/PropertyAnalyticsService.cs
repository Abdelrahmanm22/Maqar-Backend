using Maqar.Domain.Enums;
using Maqar.Domain.Repositories;
using Maqar.Domain.Services.Analytics;

namespace Maqar.Services.Analytics
{
    public class PropertyAnalyticsService : IPropertyAnalyticsService
    {
        private readonly IPropertyStatsRepository _stats;

        public PropertyAnalyticsService(IPropertyStatsRepository stats) => _stats = stats;

        public Task<EventTotals> GetTotalsAsync(int days, string? ownerScope) =>
            _stats.GetEventTotalsAsync(Since(days), ownerScope);

        public Task<IReadOnlyList<TopProperty>> GetMostViewedAsync(int days, string? ownerScope, int take) =>
            _stats.GetTopAsync(new TopPropertiesQuery(Since(days), ownerScope, take, PropertyEventType.View));

        public Task<IReadOnlyList<TopProperty>> GetMostContactedAsync(int days, string? ownerScope, int take) =>
            _stats.GetTopAsync(new TopPropertiesQuery(Since(days), ownerScope, take, PropertyEventType.WhatsAppClick, PropertyEventType.CallClick));

        public async Task<(EventTotals Recent, EventTotals AllTime)> GetPropertyTotalsAsync(int propertyId, int days) =>
            (await _stats.GetEventTotalsAsync(Since(days), ownerScope: null, propertyId),
             await _stats.GetEventTotalsAsync(since: null, ownerScope: null, propertyId));

        private static DateTime Since(int days) => DateTime.UtcNow.AddDays(-days);
    }
}
