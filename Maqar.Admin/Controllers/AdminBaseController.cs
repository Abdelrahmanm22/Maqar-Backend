using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maqar.Domain.Services;

namespace Maqar.Admin.Controllers
{
    /// <summary>
    /// Base for the dashboard's CRUD controllers. Holds only HTTP-shaped helpers — turning a service result
    /// into the <c>{ success, message }</c> JSON the AJAX flows expect. Business rules stay in services and
    /// view-model building in builders; nothing here touches the database.
    /// </summary>
    [Authorize]
    public abstract class AdminBaseController : Controller
    {
        /// <summary>The signed-in user's id. Every action is [Authorize]d, so it is always present.</summary>
        protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary><c>{ success: true, message: successMessage }</c>, or the service's first error.</summary>
        protected JsonResult ToJson(OperationResult result, string successMessage) =>
            Json(result.Succeeded
                ? new { success = true, message = successMessage }
                : new { success = false, message = result.Error });
    }
}
