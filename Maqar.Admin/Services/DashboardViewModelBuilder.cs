using Maqar.Admin.ViewModels;
using Maqar.Domain.Services.Analytics;

namespace Maqar.Admin.Services
{
    public interface IDashboardViewModelBuilder
    {
        /// <summary><paramref name="ownerScope"/> as from PropertyScope — brokers get numbers for their own listings only.</summary>
        Task<DashboardViewModel> BuildAsync(bool canViewProperties, string? ownerScope);
    }

    public class DashboardViewModelBuilder : IDashboardViewModelBuilder
    {
        private const int TopCount = 5;

        private readonly IPropertyAnalyticsService _analytics;

        public DashboardViewModelBuilder(IPropertyAnalyticsService analytics) => _analytics = analytics;

        public async Task<DashboardViewModel> BuildAsync(bool canViewProperties, string? ownerScope)
        {
            if (!canViewProperties)
                return new DashboardViewModel();

            return new DashboardViewModel
            {
                ShowStats = true,
                IsScoped = ownerScope is not null,
                Totals = await _analytics.GetTotalsAsync(DashboardViewModel.Days, ownerScope),
                MostViewed = await _analytics.GetMostViewedAsync(DashboardViewModel.Days, ownerScope, TopCount),
                MostContacted = await _analytics.GetMostContactedAsync(DashboardViewModel.Days, ownerScope, TopCount),
            };
        }
    }
}
