using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.Properties
{
    /// <summary>
    /// Every listing in an area, soft-deleted ones included — they still hold the Restrict FK, so deleting
    /// the area would fail in SQL if only live listings were counted.
    /// </summary>
    public class PropertiesInAreaSpec : BaseSpecifications<Property>
    {
        public PropertiesInAreaSpec(int areaId) : base(x => x.AreaId == areaId)
        {
            IgnoreQueryFilters = true;
        }
    }
}
