using Maqar.Domain;
using Maqar.Domain.Enums;
using Maqar.Domain.Models;
using Maqar.Domain.Services;
using Maqar.Domain.Services.ContactMessages;
using Maqar.Domain.Specifications.ContactMessages;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Services.ContactMessages
{
    public class ContactMessageService : IContactMessageService
    {
        private const string NotFound = "Message not found.";

        private readonly IUnitOfWork _unitOfWork;

        public ContactMessageService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<OperationResult<int>> CreateAsync(string name, string email, string phone, string message, int? propertyId)
        {
            if (propertyId is not null
                && await _unitOfWork.Repository<Property>().CountAsync(new PublicPropertyCountByIdSpec(propertyId.Value)) == 0)
            {
                return OperationResult<int>.Fail(OwnerScope.NotFoundMessage);
            }

            var contact = new ContactMessage
            {
                Name = name.Trim(),
                Email = email.Trim(),
                Phone = phone.Trim(),
                Message = message.Trim(),
                PropertyId = propertyId,
            };

            await _unitOfWork.Repository<ContactMessage>().AddAsync(contact);
            await _unitOfWork.CompleteAsync();
            return OperationResult<int>.Ok(contact.Id);
        }

        public async Task<PagedResult<ContactMessage>> GetPagedAsync(ContactMessageSpecParams specParams, string? ownerScope)
        {
            var repo = _unitOfWork.Repository<ContactMessage>();
            var data = await repo.GetAllAsync(new ContactMessageWithFiltersSpec(specParams, ownerScope));
            var count = await repo.CountAsync(new ContactMessageCountSpec(specParams, ownerScope));

            return new PagedResult<ContactMessage>(specParams.PageIndex, specParams.PageSize, count, data.ToList());
        }

        public Task<int> CountNewAsync(string? ownerScope) =>
            _unitOfWork.Repository<ContactMessage>().CountAsync(new ContactMessageNewCountSpec(ownerScope));

        public async Task<ContactMessage?> GetAsync(int id, string? ownerScope, bool markRead)
        {
            var message = await _unitOfWork.Repository<ContactMessage>().GetByIdAsync(new ContactMessageWithPropertySpec(id, ownerScope));

            if (message is not null && markRead && message.Status == ContactMessageStatus.New)
            {
                message.Status = ContactMessageStatus.Read;
                await _unitOfWork.CompleteAsync();
            }

            return message;
        }

        public async Task<OperationResult> ChangeStatusAsync(int id, string? ownerScope, ContactMessageStatus status)
        {
            if (!Enum.IsDefined(status))
                return OperationResult.Fail("Unknown status.");

            var message = await _unitOfWork.Repository<ContactMessage>().GetByIdAsync(new ContactMessageWithPropertySpec(id, ownerScope));
            if (message is null)
                return OperationResult.Fail(NotFound);

            message.Status = status;
            await _unitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }

        public async Task<OperationResult> DeleteAsync(int id, string? ownerScope)
        {
            var message = await _unitOfWork.Repository<ContactMessage>().GetByIdAsync(new ContactMessageWithPropertySpec(id, ownerScope));
            if (message is null)
                return OperationResult.Fail(NotFound);

            _unitOfWork.Repository<ContactMessage>().Delete(message);
            await _unitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }
    }
}
