using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.Features
{
    /// <summary>The features a property form selected — ordering, key and active checks are the generic Lookup specs.</summary>
    public class FeaturesByIdsSpec : BaseSpecifications<Feature>
    {
        public FeaturesByIdsSpec(IReadOnlyCollection<int> ids) : base(f => ids.Contains(f.Id))
        {
        }
    }
}
