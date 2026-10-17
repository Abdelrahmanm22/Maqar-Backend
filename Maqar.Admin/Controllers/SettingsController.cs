using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maqar.Admin.ViewModels;
using Maqar.Domain.Authorization;
using Maqar.Domain.Services.Settings;

namespace Maqar.Admin.Controllers
{
    /// <summary>The singleton Site Settings row — the public site's contact details (GET /api/site).</summary>
    public class SettingsController : AdminBaseController
    {
        private readonly ISiteSettingsService _settings;
        private readonly IMapper _mapper;

        public SettingsController(ISiteSettingsService settings, IMapper mapper)
        {
            _settings = settings;
            _mapper = mapper;
        }

        [Authorize(Policy = Permissions.Settings.View)]
        public async Task<IActionResult> Edit() => View(_mapper.Map<SiteSettingsViewModel>(await _settings.GetAsync()));

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Settings.Edit)]
        public async Task<IActionResult> Edit(SiteSettingsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var settings = await _settings.GetAsync();
            _mapper.Map(model, settings);
            await _settings.UpdateAsync(settings);

            TempData["Success"] = "Site settings saved. The public site shows them on its next load.";
            return RedirectToAction(nameof(Edit));
        }
    }
}
