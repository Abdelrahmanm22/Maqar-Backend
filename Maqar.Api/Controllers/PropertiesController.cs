using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Maqar.Api.Dtos;
using Maqar.Api.Errors;
using Maqar.Api.Helpers;
using Maqar.Api.Mapping;
using Maqar.Api.Options;
using Maqar.Domain.Enums;
using Maqar.Domain.Models;
using Maqar.Domain.Services.Lookups;
using Maqar.Domain.Services.Public;
using Maqar.Domain.Services.Settings;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Api.Controllers
{
    /// <summary>Published listings for the public site. Drafts, sold, archived and deleted listings answer 404.</summary>
    public class PropertiesController : ApiBaseController
    {
        private const int MaxFeatured = 12;
        private const int MaxSimilar = 8;

        private readonly IPublicPropertyService _properties;
        private readonly IPropertyEventService _events;
        private readonly ISiteSettingsService _site;
        private readonly ILookupService<Area> _areas;
        private readonly ILookupService<Feature> _features;
        private readonly MediaOptions _media;
        private readonly TrackingOptions _tracking;

        public PropertiesController(
            IPublicPropertyService properties,
            IPropertyEventService events,
            ISiteSettingsService site,
            ILookupService<Area> areas,
            ILookupService<Feature> features,
            IOptions<MediaOptions> media,
            IOptions<TrackingOptions> tracking)
        {
            _properties = properties;
            _events = events;
            _site = site;
            _areas = areas;
            _features = features;
            _media = media.Value;
            _tracking = tracking.Value;
        }

        [HttpGet]
        [EndpointSummary("Search listings")]
        [EndpointDescription("Published listings filtered, sorted and paged on the server. Query names match the frontend's URL search params.")]
        [ProducesResponseType<PagedResponse<PropertyDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiValidationErrorResponse>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResponse<PropertyDto>>> Search([FromQuery] PublicPropertySpecParams query)
        {
            var page = await _properties.GetPagedAsync(query);
            return Ok(page.ToDto(_media.BaseUrl, await _site.GetAsync()));
        }

        [HttpGet("featured")]
        [EndpointSummary("Featured listings")]
        [EndpointDescription("Published listings marked featured, newest first. take: 1–12, default 6.")]
        [ProducesResponseType<IReadOnlyList<PropertyDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<PropertyDto>>> Featured([FromQuery] int take = 6)
        {
            var listings = await _properties.GetFeaturedAsync(Math.Clamp(take, 1, MaxFeatured));
            return Ok(listings.ToDtos(_media.BaseUrl, await _site.GetAsync()));
        }

        [HttpGet("{id:int}")]
        [EndpointSummary("Listing details")]
        [ProducesResponseType<PropertyDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PropertyDto>> Get(int id)
        {
            var property = await _properties.GetAsync(id);
            if (property is null)
                return NotFound(new ApiResponse(404, "Property not found."));

            return Ok(property.ToDto(_media.BaseUrl, await _site.GetAsync()));
        }

        [HttpGet("{id:int}/similar")]
        [EndpointSummary("Similar listings")]
        [EndpointDescription("Same listing type, and the same area or property type, newest first. take: 1–8, default 4.")]
        [ProducesResponseType<IReadOnlyList<PropertyDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<PropertyDto>>> Similar(int id, [FromQuery] int take = 4)
        {
            var similar = await _properties.GetSimilarAsync(id, Math.Clamp(take, 1, MaxSimilar));
            if (similar is null)
                return NotFound(new ApiResponse(404, "Property not found."));

            return Ok(similar.ToDtos(_media.BaseUrl, await _site.GetAsync()));
        }

        [HttpGet("filters")]
        [EndpointSummary("Filter options and bounds")]
        [EndpointDescription("Active areas and features, every enum key, and min/max price (per listing type) and size of published listings.")]
        [ProducesResponseType<FilterMetadataDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<FilterMetadataDto>> Filters()
        {
            var bounds = await _properties.GetBoundsAsync();

            return Ok(new FilterMetadataDto(
                Areas: (await _areas.GetAllAsync(activeOnly: true)).Select(a => a.ToLookupDto()).ToList(),
                Features: (await _features.GetAllAsync(activeOnly: true)).Select(f => f.ToLookupDto()).ToList(),
                PropertyTypes: Enum.GetValues<PropertyType>(),
                ListingTypes: Enum.GetValues<ListingType>(),
                RentalUnits: Enum.GetValues<RentalUnit>(),
                Finishings: Enum.GetValues<FinishingType>(),
                Price: new PriceBoundsDto(bounds.SalePrice.ToDto(), bounds.RentPrice.ToDto()),
                Size: bounds.Size.ToDto()));
        }

        [HttpPost("{id:int}/events")]
        [EnableRateLimiting(RateLimits.Events)]
        [EndpointSummary("Track a view, WhatsApp tap or call tap")]
        [EndpointDescription("Always 204 on a published listing. Repeats by the same visitor within 30 minutes are accepted but not counted.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ApiValidationErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse>(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Track(int id, PropertyEventRequestDto request)
        {
            var type = request.Type switch
            {
                PropertyEventRequestDto.Whatsapp => PropertyEventType.WhatsAppClick,
                PropertyEventRequestDto.Call => PropertyEventType.CallClick,
                _ => PropertyEventType.View,
            };

            var result = await _events.RecordAsync(id, type, VisitorHash.Compute(HttpContext, _tracking.HashSalt));
            if (!result.Succeeded)
                return NotFound(new ApiResponse(404, "Property not found."));

            return NoContent();
        }
    }
}
