using Microsoft.AspNetCore.Mvc;
using Maqar.Api.Dtos;
using Maqar.Api.Mapping;
using Maqar.Domain.Services.Settings;

namespace Maqar.Api.Controllers
{
    public class SiteController : ApiBaseController
    {
        private readonly ISiteSettingsService _site;

        public SiteController(ISiteSettingsService site) => _site = site;

        [HttpGet]
        [EndpointSummary("Site contact details")]
        [EndpointDescription("Company phone, WhatsApp, email, address and social links, managed from the admin dashboard (Settings).")]
        [ProducesResponseType<SiteSettingsDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<SiteSettingsDto>> Get() => Ok((await _site.GetAsync()).ToDto());
    }
}
