using Maqar.Domain.Services;

namespace Maqar.Admin.Helpers
{
    /// <summary>
    /// Opened upload streams as the service layer takes them (<see cref="FileUpload"/>, never IFormFile).
    /// Disposing the batch closes every stream: <c>using var uploads = model.NewImages.ToUploads();</c>
    /// </summary>
    public sealed class FileUploadBatch : IDisposable
    {
        public FileUploadBatch(IReadOnlyList<FileUpload> files) => Files = files;

        public IReadOnlyList<FileUpload> Files { get; }

        public void Dispose()
        {
            foreach (var file in Files)
                file.Content.Dispose();
        }
    }

    public static class FormFileExtensions
    {
        /// <summary>Skips empty parts — an untouched file input still posts one zero-length entry.</summary>
        public static FileUploadBatch ToUploads(this IEnumerable<IFormFile>? files) =>
            new((files ?? Enumerable.Empty<IFormFile>())
                .Where(f => f.Length > 0)
                .Select(f => new FileUpload(f.OpenReadStream(), f.FileName, f.Length))
                .ToList());
    }
}
