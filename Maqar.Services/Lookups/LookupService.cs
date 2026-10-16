using Maqar.Domain;
using Maqar.Domain.Models;
using Maqar.Domain.Services;
using Maqar.Domain.Services.Lookups;
using Maqar.Domain.Specifications.Lookups;

namespace Maqar.Services.Lookups
{
    /// <summary>
    /// The shared lookup CRUD. Subclasses supply only what differs: <see cref="EntityName"/>,
    /// <see cref="CountUsageAsync"/>, and whether usage blocks a delete (<see cref="ValidateDeleteAsync"/>).
    /// </summary>
    public abstract class LookupService<T> : ILookupService<T> where T : BaseModel, ILookupEntity
    {
        protected readonly IUnitOfWork UnitOfWork;

        protected LookupService(IUnitOfWork unitOfWork) => UnitOfWork = unitOfWork;

        /// <summary>Lower-case singular for messages, e.g. "area".</summary>
        protected abstract string EntityName { get; }

        public abstract Task<int> CountUsageAsync(int id);

        /// <summary>An error message when the row can't be deleted, otherwise null. Default: always deletable.</summary>
        protected virtual Task<string?> ValidateDeleteAsync(T entity) => Task.FromResult<string?>(null);

        public async Task<IReadOnlyList<T>> GetAllAsync(bool activeOnly = false) =>
            (await UnitOfWork.Repository<T>().GetAllAsync(new LookupOrderedSpec<T>(activeOnly))).ToList();

        public async Task<T?> GetAsync(int id) =>
            await UnitOfWork.Repository<T>().GetByIdAsync(new LookupByIdSpec<T>(id));

        public async Task<bool> IsKeyAvailableAsync(string key, int excludeId = 0) =>
            string.IsNullOrWhiteSpace(key)
            || await UnitOfWork.Repository<T>().CountAsync(new LookupByKeySpec<T>(key.Trim(), excludeId)) == 0;

        public async Task<int> NextDisplayOrderAsync() =>
            (await GetAllAsync()).Select(x => x.DisplayOrder).DefaultIfEmpty(0).Max() + 1;

        public async Task<OperationResult> CreateAsync(T entity)
        {
            entity.Key = entity.Key.Trim();
            if (!await IsKeyAvailableAsync(entity.Key))
                return KeyTaken();

            await UnitOfWork.Repository<T>().AddAsync(entity);
            await UnitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }

        public async Task<OperationResult> UpdateAsync(T entity)
        {
            entity.Key = entity.Key.Trim();
            if (!await IsKeyAvailableAsync(entity.Key, entity.Id))
                return KeyTaken();

            // Already tracked by GetAsync — CompleteAsync persists the caller's changes.
            await UnitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }

        public async Task<OperationResult> DeleteAsync(int id)
        {
            var entity = await GetAsync(id);
            if (entity is null)
                return OperationResult.Fail($"{Capitalized} not found.");

            var error = await ValidateDeleteAsync(entity);
            if (error is not null)
                return OperationResult.Fail(error);

            UnitOfWork.Repository<T>().Delete(entity);
            await UnitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }

        // Same wording as the view models' [Remote] message, so client and server read alike.
        private OperationResult KeyTaken() => OperationResult.Fail($"Another {EntityName} already uses this key.");

        private string Capitalized => char.ToUpperInvariant(EntityName[0]) + EntityName[1..];
    }
}
