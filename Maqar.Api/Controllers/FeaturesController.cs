using Microsoft.AspNetCore.Mvc;
using Maqar.Api.Dtos;
using Maqar.Api.Mapping;
using Maqar.Domain.Models;
using Maqar.Domain.Services.Lookups;

namespace Maqar.Api.Controllers
{
    public class FeaturesController : ApiBaseController
    {
        private readonly ILookupService<Feature> _features;

        public FeaturesController(ILookupService<Feature> features) => _features = features;

        [HttpGet]
        [EndpointSummary("Features")]
        [EndpointDescription("Active amenities in display order. key is what listings' features array contains.")]
        [ProducesResponseType<IReadOnlyList<LookupDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<LookupDto>>> GetAll() =>
            Ok((await _features.GetAllAsync(activeOnly: true)).Select(f => f.ToLookupDto()).ToList());
    }
}
