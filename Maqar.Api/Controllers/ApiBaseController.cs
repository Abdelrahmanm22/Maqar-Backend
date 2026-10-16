using Microsoft.AspNetCore.Mvc;

namespace Maqar.Api.Controllers
{
    /// <summary>Base for every public endpoint: <c>api/[controller]</c> routing and [ApiController] validation.</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ApiBaseController : ControllerBase
    {
    }
}
