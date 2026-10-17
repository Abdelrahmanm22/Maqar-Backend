using System.Text.Json;
using System.Threading.RateLimiting;
using Maqar.Api.Errors;
using Microsoft.AspNetCore.RateLimiting;

namespace Maqar.Api
{
    /// <summary>
    /// Per-IP fixed-window limits on the two anonymous write endpoints. A rejected request gets a 429
    /// <see cref="ApiResponse"/> (the same shape as every other error) and a Retry-After header.
    /// </summary>
    public static class RateLimits
    {
        public const string Contact = "contact";
        public const string Events = "events";

        public const int ContactPermitLimit = 5;
        public static readonly TimeSpan ContactWindow = TimeSpan.FromMinutes(10);

        public const int EventsPermitLimit = 60;
        public static readonly TimeSpan EventsWindow = TimeSpan.FromMinutes(1);

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static IServiceCollection AddMaqarRateLimits(this IServiceCollection services) =>
            services.AddRateLimiter(options =>
            {
                options.AddPolicy(Contact, context => PerIp(context, ContactPermitLimit, ContactWindow));
                options.AddPolicy(Events, context => PerIp(context, EventsPermitLimit, EventsWindow));

                options.OnRejected = async (rejected, cancellationToken) =>
                {
                    var response = rejected.HttpContext.Response;
                    response.StatusCode = StatusCodes.Status429TooManyRequests;
                    response.ContentType = "application/json";

                    if (rejected.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

                    await response.WriteAsync(JsonSerializer.Serialize(new ApiResponse(429), JsonOptions), cancellationToken);
                };
            });

        private static RateLimitPartition<string> PerIp(HttpContext context, int permitLimit, TimeSpan window) =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    QueueLimit = 0,
                });
    }
}
