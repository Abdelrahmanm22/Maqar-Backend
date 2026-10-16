using System.Security.Claims;
using Maqar.Domain.Services;

namespace Maqar.Admin.Services
{
    /// <summary>
    /// <see cref="ICurrentUserService"/> over the HTTP context. Lives in the web project because that
    /// is where IHttpContextAccessor belongs; Maqar.Repository consumes only the interface.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

        private ClaimsPrincipal? User => _accessor.HttpContext?.User;

        public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

        // Identity.Name is the username; there is no full-name claim in this app's cookie.
        public string? UserName => User?.Identity?.Name;
    }
}
