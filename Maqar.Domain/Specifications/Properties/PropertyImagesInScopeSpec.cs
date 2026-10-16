using Maqar.Domain.Models;

namespace Maqar.Domain.Specifications.Properties
{
    /// <summary>Every image of a property, ordered — but only when that property is inside the caller's scope.</summary>
    public class PropertyImagesInScopeSpec : BaseSpecifications<PropertyImage>
    {
        public PropertyImagesInScopeSpec(int propertyId, string? ownerScope)
            : base(i => i.PropertyId == propertyId && (ownerScope == null || i.Property.CreatedById == ownerScope))
        {
            SetOrderBy(i => i.DisplayOrder);
        }
    }
}
