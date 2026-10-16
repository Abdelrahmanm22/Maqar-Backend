using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Maqar.Domain.Authorization;

namespace Maqar.Admin.Authorization
{
    /// <summary>
    /// Turns a permission value (e.g. "Users.View") into an authorization policy on demand, so
    /// controllers can write <c>[Authorize(Policy = Permissions.Users.View)]</c> without every
    /// permission being registered up front. Unknown names fall through to the default provider,
    /// which makes a typo fail loudly at request time instead of silently returning 403.
    /// </summary>
    public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallback;
        private readonly ConcurrentDictionary<string, AuthorizationPolicy> _cache = new(StringComparer.Ordinal);

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
            => _fallback = new DefaultAuthorizationPolicyProvider(options);

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (PermissionCatalog.IsValid(policyName))
            {
                var policy = _cache.GetOrAdd(policyName, name =>
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddRequirements(new PermissionRequirement(name))
                        .Build());

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return _fallback.GetPolicyAsync(policyName);
        }
    }
}
