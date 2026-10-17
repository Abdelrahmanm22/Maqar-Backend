using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maqar.Admin.Helpers;
using Maqar.Admin.Models;
using Maqar.Admin.Services;
using Maqar.Domain.Authorization;

namespace Maqar.Admin.Controllers
{
    public class HomeController : AdminBaseController
    {
        private readonly IDashboardViewModelBuilder _dashboard;
        private readonly IAuthorizationService _authorization;

        public HomeController(IDashboardViewModelBuilder dashboard, IAuthorizationService authorization)
        {
            _dashboard = dashboard;
            _authorization = authorization;
        }

        public async Task<IActionResult> Index()
        {
            var canViewProperties = (await _authorization.AuthorizeAsync(User, Permissions.Properties.View)).Succeeded;
            var scope = await _authorization.GetPropertyOwnerScopeAsync(User);

            return View(await _dashboard.BuildAsync(canViewProperties, scope));
        }

        // /Home/Error is the UseExceptionHandler target — requiring auth here would redirect an
        // anonymous user to Login and swallow the real error.
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
