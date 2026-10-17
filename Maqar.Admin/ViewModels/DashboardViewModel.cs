using Maqar.Domain.Services.Analytics;

namespace Maqar.Admin.ViewModels
{
    public class DashboardViewModel
    {
        public const int Days = 30;

        /// <summary>False for users without Properties.View — the welcome card shows, the stats don't.</summary>
        public bool ShowStats { get; set; }

        /// <summary>True when the numbers cover only the user's own listings (no Properties.ViewAll).</summary>
        public bool IsScoped { get; set; }

        public EventTotals Totals { get; set; } = new(0, 0, 0);
        public IReadOnlyList<TopProperty> MostViewed { get; set; } = Array.Empty<TopProperty>();
        public IReadOnlyList<TopProperty> MostContacted { get; set; } = Array.Empty<TopProperty>();
    }

    /// <summary>A listing's engagement, shown on Property/Details.</summary>
    public sealed record PropertyStatsViewModel(int Days, EventTotals Recent, EventTotals AllTime);
}
