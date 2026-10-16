namespace Maqar.Domain.Services.Properties
{
    /// <summary>
    /// What counts as a valid listing image, in one place. The Dropzone options in property-images.js and
    /// the [RequestSizeLimit] on the upload actions mirror these, but only this is trusted.
    /// </summary>
    public static class PropertyImageRules
    {
        public const string Folder = "Properties";

        public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public const long MaxFileBytes = 5 * 1024 * 1024;

        public const int MaxImagesPerProperty = 15;

        /// <summary>
        /// Set as [RequestSizeLimit] + [RequestFormLimits(MultipartBodyLengthLimit)] on every action that
        /// accepts images. Kestrel's default (~30MB) kills the request before model binding, so no
        /// validation message would ever reach the form. Comfortably above 15 × 5MB.
        /// </summary>
        public const long MaxRequestBytes = 100 * 1024 * 1024;

        /// <summary>The rejection reason, or null when the file is acceptable.</summary>
        public static string? Reject(string fileName, long length)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                return $"\"{fileName}\" isn't a JPG, PNG or WebP image.";
            if (length > MaxFileBytes)
                return $"\"{fileName}\" is larger than 5MB.";
            if (length == 0)
                return $"\"{fileName}\" is empty.";

            return null;
        }

        public static string Url(string storedFileName) => $"/Files/{Folder}/{storedFileName}";

        public static string StoredName(string url) => Path.GetFileName(url);
    }
}
