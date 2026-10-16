using Maqar.Domain.Enums;
using Maqar.Domain.Models;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Domain.Services.Properties
{
    /// <summary>Everything an admin can set on a listing — the form's payload, free of web types.</summary>
    public sealed record PropertyInput(
        PropertyType Type,
        ListingType ListingType,
        RentalUnit? RentalUnit,
        decimal Price,
        bool IsNegotiable,
        int? Bedrooms,
        int? Bathrooms,
        decimal Size,
        FinishingType? Finishing,
        int? Floor,
        string TitleAr,
        string TitleEn,
        string DescriptionAr,
        string DescriptionEn,
        string LocationAr,
        string LocationEn,
        int AreaId,
        decimal? Latitude,
        decimal? Longitude,
        IReadOnlyCollection<int> FeatureIds);

    public enum ImageMoveDirection { Up, Down }

    /// <summary>
    /// Listing management. Every member takes an <c>ownerScope</c> (see <see cref="OwnerScope"/>): null for
    /// users holding Properties.ViewAll, otherwise the caller's id — and anything outside it is "not found".
    /// </summary>
    public interface IPropertyService
    {
        Task<PagedResult<Property>> GetPagedAsync(PropertySpecParams specParams, string? ownerScope);

        /// <summary>The listing with Area, Images, Features and CreatedBy — or null when missing or out of scope.</summary>
        Task<Property?> GetAsync(int id, string? ownerScope);

        Task<OperationResult<int>> CreateAsync(PropertyInput input, IReadOnlyList<FileUpload> images, string createdById);

        Task<OperationResult> UpdateAsync(int id, string? ownerScope, PropertyInput input, IReadOnlyList<FileUpload> newImages);

        /// <summary>Soft delete — the row and its image files stay, hidden by the query filter.</summary>
        Task<OperationResult> DeleteAsync(int id, string? ownerScope);

        Task<OperationResult> ChangeStatusAsync(int id, string? ownerScope, PropertyStatus status);

        Task<OperationResult> SetFeaturedAsync(int id, string? ownerScope, bool isFeatured);

        Task<OperationResult> SetCoverAsync(int propertyId, int imageId, string? ownerScope);

        Task<OperationResult> RemoveImageAsync(int propertyId, int imageId, string? ownerScope);

        Task<OperationResult> MoveImageAsync(int propertyId, int imageId, ImageMoveDirection direction, string? ownerScope);
    }
}
