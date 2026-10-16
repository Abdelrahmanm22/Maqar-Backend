using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maqar.Admin.ViewModels;
using Maqar.Domain.Authorization;
using Maqar.Domain.Models;
using Maqar.Domain.Services.Lookups;

namespace Maqar.Admin.Controllers
{
    /// <summary>Districts a listing can be in. Rules (unique key, delete only when unused) live in <c>AreaService</c>.</summary>
    public class AreaController : AdminBaseController
    {
        private readonly ILookupService<Area> _areas;
        private readonly IMapper _mapper;

        public AreaController(ILookupService<Area> areas, IMapper mapper)
        {
            _areas = areas;
            _mapper = mapper;
        }

        [Authorize(Policy = Permissions.Areas.View)]
        public async Task<IActionResult> Index()
        {
            var rows = new List<AreaViewModel>();
            foreach (var area in await _areas.GetAllAsync())
                rows.Add(await ToViewModelAsync(area));

            return View(rows);
        }

        [Authorize(Policy = Permissions.Areas.Create)]
        public async Task<IActionResult> Create() =>
            View(new AreaViewModel { DisplayOrder = await _areas.NextDisplayOrderAsync() });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Areas.Create)]
        public async Task<IActionResult> Create(AreaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _areas.CreateAsync(_mapper.Map<Area>(model));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(model.Key), result.Error);
                return View(model);
            }

            TempData["Success"] = "Area created.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = Permissions.Areas.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var area = await _areas.GetAsync(id);
            if (area is null)
                return NotFound();

            return View(await ToViewModelAsync(area));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Areas.Edit)]
        public async Task<IActionResult> Edit(AreaViewModel model)
        {
            var area = await _areas.GetAsync(model.Id);
            if (area is null)
                return NotFound();

            if (ModelState.IsValid)
            {
                _mapper.Map(model, area);
                var result = await _areas.UpdateAsync(area);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Area updated.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(nameof(model.Key), result.Error);
            }

            model.PropertyCount = await _areas.CountUsageAsync(model.Id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Areas.Delete)]
        public async Task<IActionResult> Delete(int id) =>
            ToJson(await _areas.DeleteAsync(id), "Area deleted.");

        // View, not Create — the Edit form calls this too.
        [AcceptVerbs("GET", "POST")]
        [Authorize(Policy = Permissions.Areas.View)]
        public async Task<IActionResult> IsKeyAvailable(string key, int id) =>
            Json(await _areas.IsKeyAvailableAsync(key, id));

        private async Task<AreaViewModel> ToViewModelAsync(Area area)
        {
            var vm = _mapper.Map<AreaViewModel>(area);
            vm.PropertyCount = await _areas.CountUsageAsync(area.Id);
            return vm;
        }
    }
}
