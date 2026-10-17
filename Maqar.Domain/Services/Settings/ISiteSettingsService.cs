using Maqar.Domain.Models;

namespace Maqar.Domain.Services.Settings
{
    public interface ISiteSettingsService
    {
        /// <summary>The singleton row (seeded by migration, so it always exists).</summary>
        Task<SiteSettings> GetAsync();

        /// <summary>Saves the row loaded by <see cref="GetAsync"/> after the caller changed it.</summary>
        Task UpdateAsync(SiteSettings settings);
    }
}
