using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.Properties
{
    /// <summary>Live listings that offer a feature — for the "used by N listings" count on the Features screens.</summary>
    public class PropertiesWithFeatureSpec : BaseSpecifications<Property>
    {
        public PropertiesWithFeatureSpec(int featureId) : base(x => x.Features.Any(f => f.Id == featureId))
        {
        }
    }
}
