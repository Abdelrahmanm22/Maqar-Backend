using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Maqar.Domain.Authorization;

namespace Maqar.Admin.Helpers
{
    /// <summary>
    /// Resolves the <c>ownerScope</c> every property / inquiry service call takes: <c>null</c> (everything)
    /// when the user holds Properties.ViewAll — SuperAdmin passes through the handler — otherwise the
    /// user's own id. Goes through <see cref="IAuthorizationService"/>, never <c>User.HasClaim</c>,
    /// because SuperAdmin carries no permission claims.
    /// </summary>
    public static class PropertyScope
    {
        public static async Task<string?> GetPropertyOwnerScopeAsync(this IAuthorizationService authorization, ClaimsPrincipal user)
        {
            var canViewAll = (await authorization.AuthorizeAsync(user, Permissions.Properties.ViewAll)).Succeeded;
            return canViewAll ? null : user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
