using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maqar.Admin.ViewModels;
using Maqar.Domain.Authorization;
using Maqar.Domain.Models;
using Maqar.Domain.Services.Lookups;

namespace Maqar.Admin.Controllers
{
    /// <summary>The amenities catalogue. Rules (unique key, delete cascades off listings) live in <c>FeatureService</c>.</summary>
    public class FeatureController : AdminBaseController
    {
        private readonly ILookupService<Feature> _features;
        private readonly IMapper _mapper;

        public FeatureController(ILookupService<Feature> features, IMapper mapper)
        {
            _features = features;
            _mapper = mapper;
        }

        [Authorize(Policy = Permissions.Features.View)]
        public async Task<IActionResult> Index()
        {
            var rows = new List<FeatureViewModel>();
            foreach (var feature in await _features.GetAllAsync())
                rows.Add(await ToViewModelAsync(feature));

            return View(rows);
        }

        [Authorize(Policy = Permissions.Features.Create)]
        public async Task<IActionResult> Create() =>
            View(new FeatureViewModel { DisplayOrder = await _features.NextDisplayOrderAsync() });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Features.Create)]
        public async Task<IActionResult> Create(FeatureViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _features.CreateAsync(_mapper.Map<Feature>(model));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(model.Key), result.Error);
                return View(model);
            }

            TempData["Success"] = "Feature created.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = Permissions.Features.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var feature = await _features.GetAsync(id);
            if (feature is null)
                return NotFound();

            return View(await ToViewModelAsync(feature));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Features.Edit)]
        public async Task<IActionResult> Edit(FeatureViewModel model)
        {
            var feature = await _features.GetAsync(model.Id);
            if (feature is null)
                return NotFound();

            if (ModelState.IsValid)
            {
                _mapper.Map(model, feature);
                var result = await _features.UpdateAsync(feature);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Feature updated.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(nameof(model.Key), result.Error);
            }

            model.PropertyCount = await _features.CountUsageAsync(model.Id);
            return View(model);
        }

        /// <summary>Removing a feature also removes it from every listing that offers it (PropertyFeatures cascade).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Features.Delete)]
        public async Task<IActionResult> Delete(int id) =>
            ToJson(await _features.DeleteAsync(id), "Feature deleted.");

        [AcceptVerbs("GET", "POST")]
        [Authorize(Policy = Permissions.Features.View)]
        public async Task<IActionResult> IsKeyAvailable(string key, int id) =>
            Json(await _features.IsKeyAvailableAsync(key, id));

        private async Task<FeatureViewModel> ToViewModelAsync(Feature feature)
        {
            var vm = _mapper.Map<FeatureViewModel>(feature);
            vm.PropertyCount = await _features.CountUsageAsync(feature.Id);
            return vm;
        }
    }
}
