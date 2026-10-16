using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.Properties
{
    /// <summary>One page of the property list: filtered, scoped, sorted, with what a list row shows.</summary>
    public class PropertyWithFiltersSpec : BaseSpecifications<Property>
    {
        public PropertyWithFiltersSpec(PropertySpecParams p, string? ownerScope)
            : base(PropertyCriteria.Build(p, ownerScope))
        {
            Includes.Add(x => x.Area);
            Includes.Add(x => x.Images);   // cover thumbnail
            Includes.Add(x => x.CreatedBy!);

            switch (p.Sort)
            {
                case "oldest": SetOrderBy(x => x.CreatedAt); break;
                case "priceAsc": SetOrderBy(x => x.Price); break;
                case "priceDesc": SetOrderByDesc(x => x.Price); break;
                default: SetOrderByDesc(x => x.CreatedAt); break;
            }

            ApplyPagination((p.PageIndex - 1) * p.PageSize, p.PageSize);
        }
    }
}
