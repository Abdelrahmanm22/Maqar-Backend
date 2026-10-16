namespace Maqar.Domain.Models
{
    /// <summary>An amenity a listing can offer (elevator, pool, …).</summary>
    public class Feature : BaseModel, ILookupEntity
    {
        /// <summary>Stable key the frontend translates, e.g. "elevator".</summary>
        public string Key { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        /// <summary>Optional Boxicons class for the admin UI.</summary>
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
