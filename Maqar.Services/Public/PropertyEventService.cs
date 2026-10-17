using Maqar.Domain;
using Maqar.Domain.Enums;
using Maqar.Domain.Models;
using Maqar.Domain.Services;
using Maqar.Domain.Services.Public;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Services.Public
{
    public class PropertyEventService : IPropertyEventService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PropertyEventService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<OperationResult> RecordAsync(int propertyId, PropertyEventType type, string visitorHash)
        {
            if (!Enum.IsDefined(type))
                return OperationResult.Fail("Unknown event type.");

            var isPublic = await _unitOfWork.Repository<Property>().CountAsync(new PublicPropertyCountByIdSpec(propertyId)) > 0;
            if (!isPublic)
                return OperationResult.Fail(OwnerScope.NotFoundMessage);

            var now = DateTime.UtcNow;
            var events = _unitOfWork.Repository<PropertyEvent>();

            // Refreshes, back-navigation and double taps don't inflate the numbers.
            var since = now - IPropertyEventService.DedupWindow;
            if (await events.CountAsync(new RecentEventSpec(propertyId, type, visitorHash, since)) > 0)
                return OperationResult.Ok();

            await events.AddAsync(new PropertyEvent
            {
                PropertyId = propertyId,
                Type = type,
                VisitorHash = visitorHash,
                CreatedAt = now,
            });
            await _unitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }
    }
}
