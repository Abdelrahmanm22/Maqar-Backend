namespace Maqar.Domain.Services
{
    /// <summary>
    /// Who is making the current request. An abstraction rather than IHttpContextAccessor directly,
    /// because Maqar.Repository and Maqar.Services must stay free of ASP.NET Core — same rule that keeps IFileStorageService taking a Stream instead of IFormFile.
    /// Returns nulls outside a request (startup migrations, seeding, a future background job).
    /// </summary>
    public interface ICurrentUserService
    {
        string? UserId { get; }

        /// <summary>Display name to stamp on records (e.g. CreatedBy) — a snapshot, not a lookup.</summary>
        string? UserName { get; }
    }
}
