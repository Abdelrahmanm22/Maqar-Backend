using Microsoft.AspNetCore.Mvc;
using Maqar.Api.Dtos;
using Maqar.Api.Mapping;
using Maqar.Domain.Models;
using Maqar.Domain.Services.Lookups;

namespace Maqar.Api.Controllers
{
    public class AreasController : ApiBaseController
    {
        private readonly ILookupService<Area> _areas;

        public AreasController(ILookupService<Area> areas) => _areas = areas;

        [HttpGet]
        [EndpointSummary("Areas")]
        [EndpointDescription("Active areas in display order. key is what listings' area field and the area filter use.")]
        [ProducesResponseType<IReadOnlyList<LookupDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<LookupDto>>> GetAll() =>
            Ok((await _areas.GetAllAsync(activeOnly: true)).Select(a => a.ToLookupDto()).ToList());
    }
}
