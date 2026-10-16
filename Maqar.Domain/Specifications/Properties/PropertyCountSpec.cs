using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.Properties
{
    /// <summary>Total matches behind <see cref="PropertyWithFiltersSpec"/> — same criteria, no paging or includes.</summary>
    public class PropertyCountSpec : BaseSpecifications<Property>
    {
        public PropertyCountSpec(PropertySpecParams p, string? ownerScope)
            : base(PropertyCriteria.Build(p, ownerScope))
        {
        }
    }
}
