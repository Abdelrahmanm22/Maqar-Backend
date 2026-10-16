namespace Maqar.Domain.Models
{
    public class Area : BaseModel, ILookupEntity
    {
        /// <summary>Stable key the frontend translates and filters by, e.g. "fifthSettlement".</summary>
        public string Key { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
