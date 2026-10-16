namespace Maqar.Domain.Models
{
    /// <summary>
    /// <c>MaqarContext.SaveChanges</c> turns a Delete of this entity into an update that sets these,
    /// and a global query filter hides the row. Use <c>IgnoreQueryFilters()</c> to see deleted rows.
    /// </summary>
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}
