namespace Maqar.Domain.Services
{
    /// <summary>
    /// An uploaded file as the service layer sees it — a Stream, never IFormFile, so Maqar.Services stays
    /// web-agnostic (same rule as IFileStorageService). Controllers build it from IFormFile.OpenReadStream().
    /// </summary>
    public sealed record FileUpload(Stream Content, string FileName, long Length);
}
