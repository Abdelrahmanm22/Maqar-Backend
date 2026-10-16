namespace Maqar.Domain.Models
{
    /// <summary>
    /// A keyed, ordered, switchable lookup (Area, Feature). Lets one generic service and one set of specs
    /// handle both — key uniqueness, display order, activation — instead of a copy per entity.
    /// </summary>
    public interface ILookupEntity
    {
        int Id { get; }
        /// <summary>Stable key the public site filters and translates by.</summary>
        string Key { get; set; }
        int DisplayOrder { get; set; }
        bool IsActive { get; set; }
    }
}
