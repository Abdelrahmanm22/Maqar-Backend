using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maqar.Admin.Helpers;
using Maqar.Admin.Services;
using Maqar.Admin.ViewModels;
using Maqar.Domain.Authorization;
using Maqar.Domain.Services;
using Maqar.Domain.Services.Properties;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Admin.Controllers
{
    /// <summary>
    /// Listings. Every action resolves the caller's ownership scope first (see <see cref="PropertyScope"/>)
    /// and hands it to <see cref="IPropertyService"/>, which reports anything outside it as not found —
    /// so a broker without Properties.ViewAll can neither see nor touch another broker's listing.
    /// View models are built by <see cref="IPropertyViewModelBuilder"/>; this class stays HTTP-only.
    /// </summary>
    public class PropertyController : AdminBaseController
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyViewModelBuilder _viewModels;
        private readonly IAuthorizationService _authorization;
        private readonly IMapper _mapper;

        public PropertyController(
            IPropertyService propertyService,
            IPropertyViewModelBuilder viewModels,
            IAuthorizationService authorization,
            IMapper mapper)
        {
            _propertyService = propertyService;
            _viewModels = viewModels;
            _authorization = authorization;
            _mapper = mapper;
        }

        // ─── List & details ─────────────────────────────────────────────────────

        [Authorize(Policy = Permissions.Properties.View)]
        public async Task<IActionResult> Index([FromQuery] PropertySpecParams filters) =>
            View(await _viewModels.BuildListAsync(filters, await GetScopeAsync()));

        [Authorize(Policy = Permissions.Properties.View)]
        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyService.GetAsync(id, await GetScopeAsync());
            if (property is null)
                return NotFound();

            var canViewInquiries = (await _authorization.AuthorizeAsync(User, Permissions.ContactMessages.View)).Succeeded;
            return View(await _viewModels.BuildDetailsAsync(property, canViewInquiries));
        }

        // ─── Create ─────────────────────────────────────────────────────────────

        [Authorize(Policy = Permissions.Properties.Create)]
        public async Task<IActionResult> Create() => View(await _viewModels.BuildCreateFormAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Properties.Create)]
        [RequestSizeLimit(PropertyImageRules.MaxRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = PropertyImageRules.MaxRequestBytes)]
        public async Task<IActionResult> Create(PropertyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(await _viewModels.RefillFormAsync(model, existing: null));

            using var uploads = model.NewImages.ToUploads();
            var result = await _propertyService.CreateAsync(_mapper.Map<PropertyInput>(model), uploads.Files, CurrentUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return View(await _viewModels.RefillFormAsync(model, existing: null));
            }

            TempData["Success"] = "Property created. It is a draft until you publish it.";
            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        // ─── Edit ───────────────────────────────────────────────────────────────

        [Authorize(Policy = Permissions.Properties.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var property = await _propertyService.GetAsync(id, await GetScopeAsync());
            if (property is null)
                return NotFound();

            return View(await _viewModels.BuildEditFormAsync(property));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Properties.Edit)]
        [RequestSizeLimit(PropertyImageRules.MaxRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = PropertyImageRules.MaxRequestBytes)]
        public async Task<IActionResult> Edit(PropertyFormViewModel model)
        {
            var scope = await GetScopeAsync();

            if (ModelState.IsValid)
            {
                using var uploads = model.NewImages.ToUploads();
                var result = await _propertyService.UpdateAsync(model.Id, scope, _mapper.Map<PropertyInput>(model), uploads.Files);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Property updated.";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }

                ModelState.AddModelError(string.Empty, result.Error);
            }

            // Redisplay — existing images aren't posted back, so reload the listing (which re-checks the scope).
            var existing = await _propertyService.GetAsync(model.Id, scope);
            if (existing is null)
                return NotFound();

            return View(await _viewModels.RefillFormAsync(model, existing));
        }

        /// <summary>The image strip on Edit, re-rendered after an AJAX cover / move / remove.</summary>
        [Authorize(Policy = Permissions.Properties.Edit)]
        public async Task<IActionResult> Images(int id)
        {
            var property = await _propertyService.GetAsync(id, await GetScopeAsync());
            if (property is null)
                return NotFound();

            return PartialView("_PropertyImages", _viewModels.BuildImageStrip(property));
        }

        // ─── AJAX actions — JSON { success, message } ───────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Properties.Delete)]
        public async Task<IActionResult> Delete(int id) =>
            ToJson(await _propertyService.DeleteAsync(id, await GetScopeAsync()), "Property deleted.");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Properties.Publish)]
        public async Task<IActionResult> ChangeStatus(ChangePropertyStatusRequest request) =>
            ToJson(await _propertyService.ChangeStatusAsync(request.Id, await GetScopeAsync(), request.Status),
                $"Status changed to {PropertyBadges.ForStatus(request.Status).Label}.");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Properties.Feature)]
        public async Task<IActionResult> ToggleFeatured(int id, bool isFeatured) =>
            ToJson(await _propertyService.SetFeaturedAsync(id, await GetScopeAsync(), isFeatured),
                isFeatured ? "Added to featured listings." : "Removed from featured listings.");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Properties.Edit)]
        public async Task<IActionResult> SetCover(int id, int imageId) =>
            ToJson(await _propertyService.SetCoverAsync(id, imageId, await GetScopeAsync()), "Cover image updated.");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Properties.Edit)]
        public async Task<IActionResult> RemoveImage(int id, int imageId) =>
            ToJson(await _propertyService.RemoveImageAsync(id, imageId, await GetScopeAsync()), "Image removed.");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Properties.Edit)]
        public async Task<IActionResult> MoveImage(int id, int imageId, ImageMoveDirection direction) =>
            ToJson(await _propertyService.MoveImageAsync(id, imageId, direction, await GetScopeAsync()), "Image order updated.");

        private Task<string?> GetScopeAsync() => _authorization.GetPropertyOwnerScopeAsync(User);
    }
}
