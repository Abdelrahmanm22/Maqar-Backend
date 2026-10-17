using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Maqar.Api.Dtos;
using Maqar.Api.Errors;
using Maqar.Domain.Services.ContactMessages;

namespace Maqar.Api.Controllers
{
    public class ContactController : ApiBaseController
    {
        private readonly IContactMessageService _messages;

        public ContactController(IContactMessageService messages) => _messages = messages;

        [HttpPost]
        [EnableRateLimiting(RateLimits.Contact)]
        [EndpointSummary("Send a contact message")]
        [EndpointDescription("The contact form, or an inquiry from a listing page when propertyId is set. Lands in the admin dashboard's inbox (and the listing broker's).")]
        [ProducesResponseType<ContactCreatedDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ApiValidationErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse>(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ContactCreatedDto>> Send(ContactRequestDto request)
        {
            var result = await _messages.CreateAsync(request.Name, request.Email, request.Phone, request.Message, request.PropertyId);
            if (!result.Succeeded)
                return NotFound(new ApiResponse(404, "The listing this inquiry is about is no longer available."));

            return StatusCode(StatusCodes.Status201Created, new ContactCreatedDto(result.Value));
        }
    }
}
