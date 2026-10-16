namespace Maqar.Domain.Models
{
    public class PropertyImage : BaseModel
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;

        /// <summary>"/Files/Properties/{stored name}", as written by IFileStorageService.</summary>
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        /// <summary>The listing-card image. Keep exactly one per property — enforced in the service.</summary>
        public bool IsCover { get; set; }
    }
}
