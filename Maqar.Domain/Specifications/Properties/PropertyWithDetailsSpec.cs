using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.Properties
{
    /// <summary>
    /// One property with everything the details / edit screens need — or nothing when it is outside the
    /// caller's ownership scope, which the caller reports as "not found".
    /// </summary>
    public class PropertyWithDetailsSpec : BaseSpecifications<Property>
    {
        public PropertyWithDetailsSpec(int id, string? ownerScope, bool includeRelated = true)
            : base(x => x.Id == id && (ownerScope == null || x.CreatedById == ownerScope))
        {
            if (!includeRelated)
                return;

            Includes.Add(x => x.Area);
            Includes.Add(x => x.Images);
            Includes.Add(x => x.Features);
            Includes.Add(x => x.CreatedBy!);
            AsSplitQuery = true; // Images + Features are two collections
        }
    }
}
