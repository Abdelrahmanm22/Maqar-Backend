namespace Maqar.Domain.Models
{
    /// <summary>Timestamps stamped (UTC) by <c>MaqarContext.SaveChanges</c> — never set them by hand.</summary>
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
