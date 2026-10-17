namespace Maqar.Api.Options
{
    /// <summary><c>Media</c> section. Images are uploaded through Maqar.Admin and served from its /Files folder.</summary>
    public sealed class MediaOptions
    {
        public const string Section = "Media";

        /// <summary>Origin that serves /Files/... — the Admin app in development, a CDN or the Admin host in production.</summary>
        public string BaseUrl { get; set; } = string.Empty;
    }

    /// <summary><c>Tracking</c> section.</summary>
    public sealed class TrackingOptions
    {
        public const string Section = "Tracking";

        /// <summary>Mixed into the visitor hash so it can't be reversed by hashing known IPs. Keep it secret in production.</summary>
        public string HashSalt { get; set; } = string.Empty;
    }
}
