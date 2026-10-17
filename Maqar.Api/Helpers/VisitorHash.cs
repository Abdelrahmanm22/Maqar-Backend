using System.Security.Cryptography;
using System.Text;

namespace Maqar.Api.Helpers
{
    /// <summary>
    /// A stable, anonymous id for "the same visitor" — SHA-256 of IP + User-Agent + a server salt. Used only
    /// to de-duplicate tracking events; the raw IP is never stored.
    /// </summary>
    public static class VisitorHash
    {
        public static string Compute(HttpContext context, string salt)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers.UserAgent.ToString();

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{ip}|{userAgent}|{salt}"));
            return Convert.ToHexString(bytes); // 64 chars
        }
    }
}
