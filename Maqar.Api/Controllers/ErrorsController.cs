using Microsoft.AspNetCore.Mvc;
using Maqar.Api.Errors;

namespace Maqar.Api.Controllers
{
    /// <summary>
    /// Target of <c>UseStatusCodePagesWithReExecute("/errors/{0}")</c>: any response that leaves the
    /// pipeline with an error status and no body — an unknown route (404), a wrong verb (405), a bare
    /// 401/403 — is re-executed here and gets an <see cref="ApiResponse"/> body with its real status.
    /// No verb attribute on purpose: the re-executed request keeps its original method.
    /// </summary>
    [Route("errors/{code}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
        public ActionResult Error(int code)
        {
            return new ObjectResult(new ApiResponse(code)) { StatusCode = code };
        }
    }
}
