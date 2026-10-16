using Maqar.Domain.Models;

namespace Maqar.Domain.Services.Lookups
{
    /// <summary>
    /// CRUD for a keyed lookup (Area, Feature). The entity-specific part — how many listings use a row and
    /// whether that blocks deleting it — lives in each implementation.
    /// </summary>
    public interface ILookupService<T> where T : BaseModel, ILookupEntity
    {
        Task<IReadOnlyList<T>> GetAllAsync(bool activeOnly = false);

        Task<T?> GetAsync(int id);

        Task<bool> IsKeyAvailableAsync(string key, int excludeId = 0);

        /// <summary>One past the current highest — the default for a new row.</summary>
        Task<int> NextDisplayOrderAsync();

        /// <summary>Listings that reference the row (the number shown in lists and on the delete warning).</summary>
        Task<int> CountUsageAsync(int id);

        /// <summary>Fails when the key is taken.</summary>
        Task<OperationResult> CreateAsync(T entity);

        /// <summary>Saves an entity already loaded through <see cref="GetAsync"/> and changed by the caller. Fails when the key is taken.</summary>
        Task<OperationResult> UpdateAsync(T entity);

        Task<OperationResult> DeleteAsync(int id);
    }
}
