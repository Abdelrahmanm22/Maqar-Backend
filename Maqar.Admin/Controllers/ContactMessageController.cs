using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maqar.Admin.Helpers;
using Maqar.Admin.ViewModels;
using Maqar.Domain.Authorization;
using Maqar.Domain.Services.ContactMessages;
using Maqar.Domain.Specifications.ContactMessages;

namespace Maqar.Admin.Controllers
{
    /// <summary>
    /// The inbox for the public site's contact form and listing inquiries. Scoped like properties: without
    /// Properties.ViewAll a user sees only inquiries about their own listings.
    /// </summary>
    public class ContactMessageController : AdminBaseController
    {
        private readonly IContactMessageService _messages;
        private readonly IAuthorizationService _authorization;

        public ContactMessageController(IContactMessageService messages, IAuthorizationService authorization)
        {
            _messages = messages;
            _authorization = authorization;
        }

        [Authorize(Policy = Permissions.ContactMessages.View)]
        public async Task<IActionResult> Index([FromQuery] ContactMessageSpecParams filters)
        {
            var scope = await GetScopeAsync();

            return View(new ContactMessageListViewModel
            {
                Filters = filters,
                Page = await _messages.GetPagedAsync(filters, scope),
                NewCount = await _messages.CountNewAsync(scope),
                CanViewAll = scope is null,
            });
        }

        [Authorize(Policy = Permissions.ContactMessages.View)]
        public async Task<IActionResult> Details(int id)
        {
            // Opening a message counts as reading it — but only for someone allowed to change its status.
            var canChangeStatus = (await _authorization.AuthorizeAsync(User, Permissions.ContactMessages.ChangeStatus)).Succeeded;

            var message = await _messages.GetAsync(id, await GetScopeAsync(), markRead: canChangeStatus);
            if (message is null)
                return NotFound();

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.ContactMessages.ChangeStatus)]
        public async Task<IActionResult> ChangeStatus(ChangeMessageStatusRequest request) =>
            ToJson(await _messages.ChangeStatusAsync(request.Id, await GetScopeAsync(), request.Status),
                $"Marked {PropertyBadges.ForMessage(request.Status).Label.ToLowerInvariant()}.");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.ContactMessages.Delete)]
        public async Task<IActionResult> Delete(int id) =>
            ToJson(await _messages.DeleteAsync(id, await GetScopeAsync()), "Message deleted.");

        private Task<string?> GetScopeAsync() => _authorization.GetPropertyOwnerScopeAsync(User);
    }
}
