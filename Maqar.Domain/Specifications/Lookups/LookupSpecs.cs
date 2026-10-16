using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.Lookups
{
    /// <summary>Any lookup in display order; <paramref name="activeOnly"/> for pickers on the property form.</summary>
    public class LookupOrderedSpec<T> : BaseSpecifications<T> where T : BaseModel, ILookupEntity
    {
        public LookupOrderedSpec(bool activeOnly = false)
            : base(x => !activeOnly || x.IsActive)
        {
            SetOrderBy(x => x.DisplayOrder);
        }
    }

    public class LookupByIdSpec<T> : BaseSpecifications<T> where T : BaseModel, ILookupEntity
    {
        public LookupByIdSpec(int id) : base(x => x.Id == id)
        {
        }
    }

    /// <summary>Key uniqueness — <paramref name="excludeId"/> is the row being edited (0 on create).</summary>
    public class LookupByKeySpec<T> : BaseSpecifications<T> where T : BaseModel, ILookupEntity
    {
        public LookupByKeySpec(string key, int excludeId = 0)
            : base(x => x.Key == key && x.Id != excludeId)
        {
        }
    }
}
