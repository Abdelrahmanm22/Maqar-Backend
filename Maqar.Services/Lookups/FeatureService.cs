using Maqar.Domain;
using Maqar.Domain.Models;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Services.Lookups
{
    /// <summary>Deleting a feature is always allowed — the PropertyFeatures cascade removes it from every listing.</summary>
    public class FeatureService : LookupService<Feature>
    {
        public FeatureService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        protected override string EntityName => "feature";

        public override Task<int> CountUsageAsync(int id) =>
            UnitOfWork.Repository<Property>().CountAsync(new PropertiesWithFeatureSpec(id));
    }
}
