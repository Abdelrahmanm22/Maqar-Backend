namespace Maqar.Domain.Services
{
    /// <summary>
    /// The ownership convention every property-related query and action takes as <c>string? ownerScope</c>:
    /// <c>null</c> means unrestricted (the user holds <c>Properties.ViewAll</c>); otherwise it is the
    /// current user's id and only listings they created are visible. Anything outside the scope is reported
    /// as "not found", never "forbidden", so a broker can't probe which ids exist.
    /// </summary>
    public static class OwnerScope
    {
        public const string NotFoundMessage = "Property not found.";
    }
}
