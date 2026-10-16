using Maqar.Domain;
using Maqar.Domain.Models;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Services.Lookups
{
    public class AreaService : LookupService<Area>
    {
        public AreaService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        protected override string EntityName => "area";

        /// <summary>Soft-deleted listings included — they still hold the Restrict FK.</summary>
        public override Task<int> CountUsageAsync(int id) =>
            UnitOfWork.Repository<Property>().CountAsync(new PropertiesInAreaSpec(id));

        /// <summary>Blocked while any listing uses it; otherwise SQL would reject the delete anyway.</summary>
        protected override async Task<string?> ValidateDeleteAsync(Area entity)
        {
            var inUse = await CountUsageAsync(entity.Id);
            return inUse == 0
                ? null
                : $"{inUse} listing(s) use this area, so it can't be deleted. Deactivate it instead to hide it from new listings.";
        }
    }
}
