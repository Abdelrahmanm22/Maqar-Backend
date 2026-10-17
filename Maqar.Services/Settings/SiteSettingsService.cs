using Maqar.Domain;
using Maqar.Domain.Models;
using Maqar.Domain.Services.Settings;
using Maqar.Domain.Specifications;

namespace Maqar.Services.Settings
{
    public class SiteSettingsService : ISiteSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SiteSettingsService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<SiteSettings> GetAsync() =>
            await _unitOfWork.Repository<SiteSettings>().GetByIdAsync(new BaseSpecifications<SiteSettings>(s => s.Id == SiteSettings.SingletonId))
            ?? throw new InvalidOperationException("The SiteSettings row is missing. It is seeded by migration AddTrackingAndSiteSettings.");

        public async Task UpdateAsync(SiteSettings settings)
        {
            settings.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.CompleteAsync(); // tracked by GetAsync
        }
    }
}
