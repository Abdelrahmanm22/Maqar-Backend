using Maqar.Domain;
using Maqar.Domain.Models;
using Maqar.Domain.Repositories;
using Maqar.Domain.Services;
using Maqar.Domain.Services.Analytics;
using Maqar.Domain.Services.Public;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Services.Public
{
    public class PublicPropertyService : IPublicPropertyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPropertyStatsRepository _stats;

        public PublicPropertyService(IUnitOfWork unitOfWork, IPropertyStatsRepository stats)
        {
            _unitOfWork = unitOfWork;
            _stats = stats;
        }

        public async Task<PagedResult<Property>> GetPagedAsync(PublicPropertySpecParams query)
        {
            var repo = _unitOfWork.Repository<Property>();
            var data = await repo.GetAllAsync(new PublicPropertyWithFiltersSpec(query));
            var count = await repo.CountAsync(new PublicPropertyCountSpec(query));

            return new PagedResult<Property>(query.Page, query.PageSize, count, data.ToList());
        }

        public async Task<Property?> GetAsync(int id) =>
            await _unitOfWork.Repository<Property>().GetByIdAsync(new PublicPropertyByIdSpec(id));

        public async Task<IReadOnlyList<Property>> GetFeaturedAsync(int take) =>
            (await _unitOfWork.Repository<Property>().GetAllAsync(new FeaturedPropertiesSpec(take))).ToList();

        public async Task<IReadOnlyList<Property>?> GetSimilarAsync(int id, int take)
        {
            var source = await GetAsync(id);
            if (source is null)
                return null;

            return (await _unitOfWork.Repository<Property>().GetAllAsync(new SimilarPropertiesSpec(source, take))).ToList();
        }

        public Task<PropertyBounds> GetBoundsAsync() => _stats.GetPublishedBoundsAsync();
    }
}
