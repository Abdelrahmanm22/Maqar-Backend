using Maqar.Domain;
using Maqar.Domain.Enums;
using Maqar.Domain.Models;
using Maqar.Domain.Services;
using Maqar.Domain.Services.Properties;
using Maqar.Domain.Specifications;
using Maqar.Domain.Specifications.Features;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Services.Properties
{
    public class PropertyService : IPropertyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;

        public PropertyService(IUnitOfWork unitOfWork, IFileStorageService fileStorage)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        // ─── Reads ──────────────────────────────────────────────────────────────

        public async Task<PagedResult<Property>> GetPagedAsync(PropertySpecParams specParams, string? ownerScope)
        {
            var repo = _unitOfWork.Repository<Property>();
            var data = await repo.GetAllAsync(new PropertyWithFiltersSpec(specParams, ownerScope));
            var count = await repo.CountAsync(new PropertyCountSpec(specParams, ownerScope));

            return new PagedResult<Property>(specParams.PageIndex, specParams.PageSize, count, data.ToList());
        }

        public async Task<Property?> GetAsync(int id, string? ownerScope) =>
            await _unitOfWork.Repository<Property>().GetByIdAsync(new PropertyWithDetailsSpec(id, ownerScope));

        // ─── Create / Update ────────────────────────────────────────────────────

        public async Task<OperationResult<int>> CreateAsync(PropertyInput input, IReadOnlyList<FileUpload> images, string createdById)
        {
            var error = ValidateImages(images, existingCount: 0);
            if (error is not null)
                return OperationResult<int>.Fail(error);

            var property = new Property { CreatedById = createdById };

            var applyError = await ApplyInputAsync(property, input, currentAreaId: null);
            if (applyError is not null)
                return OperationResult<int>.Fail(applyError);

            var saved = await SaveImagesAsync(property, images);
            try
            {
                EnsureSingleCover(property.Images);
                await _unitOfWork.Repository<Property>().AddAsync(property);
                await _unitOfWork.CompleteAsync(); // one SaveChanges — already atomic
            }
            catch
            {
                DeleteFiles(saved); // the row never landed, so neither should its files
                throw;
            }

            return OperationResult<int>.Ok(property.Id);
        }

        public async Task<OperationResult> UpdateAsync(int id, string? ownerScope, PropertyInput input, IReadOnlyList<FileUpload> newImages)
        {
            var property = await GetAsync(id, ownerScope);
            if (property is null)
                return OperationResult.Fail(OwnerScope.NotFoundMessage);

            var error = ValidateImages(newImages, property.Images.Count);
            if (error is not null)
                return OperationResult.Fail(error);

            var applyError = await ApplyInputAsync(property, input, currentAreaId: property.AreaId);
            if (applyError is not null)
                return OperationResult.Fail(applyError);

            var saved = await SaveImagesAsync(property, newImages);
            try
            {
                EnsureSingleCover(property.Images);
                // Tracked by GetAsync, so no Repository.Update(): that would also mark every loaded
                // Feature as Modified and rewrite rows nobody touched.
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                DeleteFiles(saved);
                throw;
            }

            return OperationResult.Ok();
        }

        // ─── Lifecycle ──────────────────────────────────────────────────────────

        public async Task<OperationResult> DeleteAsync(int id, string? ownerScope)
        {
            var property = await _unitOfWork.Repository<Property>()
                .GetByIdAsync(new PropertyWithDetailsSpec(id, ownerScope, includeRelated: false));
            if (property is null)
                return OperationResult.Fail(OwnerScope.NotFoundMessage);

            // MaqarContext turns this into IsDeleted = true; images and files are kept for a restore.
            _unitOfWork.Repository<Property>().Delete(property);
            await _unitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }

        public async Task<OperationResult> ChangeStatusAsync(int id, string? ownerScope, PropertyStatus status)
        {
            if (!Enum.IsDefined(status))
                return OperationResult.Fail("Unknown status.");

            var property = await GetAsync(id, ownerScope);
            if (property is null)
                return OperationResult.Fail(OwnerScope.NotFoundMessage);

            if (status == PropertyStatus.Published && property.Images.Count == 0)
                return OperationResult.Fail("Add at least one image before publishing — the listing card needs a cover.");

            if (status == PropertyStatus.Rented && property.ListingType != ListingType.Rent)
                return OperationResult.Fail("Only a rental listing can be marked rented.");

            if (status == PropertyStatus.Sold && property.ListingType != ListingType.Sale)
                return OperationResult.Fail("Only a sale listing can be marked sold.");

            property.Status = status;
            await _unitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }

        public async Task<OperationResult> SetFeaturedAsync(int id, string? ownerScope, bool isFeatured)
        {
            var property = await _unitOfWork.Repository<Property>()
                .GetByIdAsync(new PropertyWithDetailsSpec(id, ownerScope, includeRelated: false));
            if (property is null)
                return OperationResult.Fail(OwnerScope.NotFoundMessage);

            property.IsFeatured = isFeatured;
            await _unitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }

        // ─── Images ─────────────────────────────────────────────────────────────

        public async Task<OperationResult> SetCoverAsync(int propertyId, int imageId, string? ownerScope)
        {
            var images = await GetImagesInScopeAsync(propertyId, ownerScope);
            var target = images.FirstOrDefault(i => i.Id == imageId);
            if (target is null)
                return OperationResult.Fail("Image not found.");

            foreach (var image in images)
                image.IsCover = image.Id == imageId;

            await _unitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }

        public async Task<OperationResult> RemoveImageAsync(int propertyId, int imageId, string? ownerScope)
        {
            var images = await GetImagesInScopeAsync(propertyId, ownerScope);
            var target = images.FirstOrDefault(i => i.Id == imageId);
            if (target is null)
                return OperationResult.Fail("Image not found.");

            var remaining = images.Where(i => i.Id != imageId).ToList();
            if (remaining.Count == 0)
            {
                var property = await _unitOfWork.Repository<Property>()
                    .GetByIdAsync(new PropertyWithDetailsSpec(propertyId, ownerScope, includeRelated: false));
                if (property?.Status == PropertyStatus.Published)
                    return OperationResult.Fail("A published listing must keep at least one image. Unpublish it first.");
            }

            _unitOfWork.Repository<PropertyImage>().Delete(target);
            EnsureSingleCover(remaining);
            Renumber(remaining);
            await _unitOfWork.CompleteAsync();

            // After the commit — a failed save must not leave a row pointing at a deleted file.
            _fileStorage.DeleteFile(PropertyImageRules.StoredName(target.ImageUrl), PropertyImageRules.Folder);
            return OperationResult.Ok();
        }

        public async Task<OperationResult> MoveImageAsync(int propertyId, int imageId, ImageMoveDirection direction, string? ownerScope)
        {
            var images = await GetImagesInScopeAsync(propertyId, ownerScope);
            var index = images.FindIndex(i => i.Id == imageId);
            if (index < 0)
                return OperationResult.Fail("Image not found.");

            var swapWith = direction == ImageMoveDirection.Up ? index - 1 : index + 1;
            if (swapWith < 0 || swapWith >= images.Count)
                return OperationResult.Ok(); // already at the edge — nothing to do

            (images[index], images[swapWith]) = (images[swapWith], images[index]);
            Renumber(images);
            await _unitOfWork.CompleteAsync();
            return OperationResult.Ok();
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        /// <summary>Copies the form onto the entity and enforces the cross-field rules. Returns an error or null.</summary>
        private async Task<string?> ApplyInputAsync(Property property, PropertyInput input, int? currentAreaId)
        {
            var area = await _unitOfWork.Repository<Area>().GetByIdAsync(new BaseSpecifications<Area>(a => a.Id == input.AreaId));
            if (area is null)
                return "Choose a valid area.";
            // An inactive area stays allowed on a listing that already uses it, so editing an old listing doesn't break.
            if (!area.IsActive && currentAreaId != area.Id)
                return $"The area \"{area.NameEn}\" is inactive. Choose another one.";

            var featureIds = input.FeatureIds.Distinct().ToList();
            var features = featureIds.Count == 0
                ? new List<Feature>()
                : (await _unitOfWork.Repository<Feature>().GetAllAsync(new FeaturesByIdsSpec(featureIds))).ToList();
            if (features.Count != featureIds.Count)
                return "One of the selected features no longer exists.";

            property.Type = input.Type;
            property.ListingType = input.ListingType;
            // The DB check constraint rejects a rental unit on a sale — clear it rather than fail.
            property.RentalUnit = input.ListingType == ListingType.Rent ? input.RentalUnit : null;
            property.Price = input.Price;
            property.IsNegotiable = input.IsNegotiable;
            property.Size = input.Size;
            property.Floor = input.Floor;
            // Land has no rooms or finishing; drop whatever the form still carried.
            var isLand = input.Type == PropertyType.Land;
            property.Bedrooms = isLand ? null : input.Bedrooms;
            property.Bathrooms = isLand ? null : input.Bathrooms;
            property.Finishing = isLand ? null : input.Finishing;
            property.TitleAr = input.TitleAr.Trim();
            property.TitleEn = input.TitleEn.Trim();
            property.DescriptionAr = input.DescriptionAr.Trim();
            property.DescriptionEn = input.DescriptionEn.Trim();
            property.LocationAr = input.LocationAr.Trim();
            property.LocationEn = input.LocationEn.Trim();
            property.AreaId = area.Id;
            property.Latitude = input.Latitude;
            property.Longitude = input.Longitude;

            property.Features.Clear();
            foreach (var feature in features)
                property.Features.Add(feature);

            return null;
        }

        private static string? ValidateImages(IReadOnlyList<FileUpload> images, int existingCount)
        {
            if (existingCount + images.Count > PropertyImageRules.MaxImagesPerProperty)
                return $"A listing can have at most {PropertyImageRules.MaxImagesPerProperty} images.";

            return images.Select(i => PropertyImageRules.Reject(i.FileName, i.Length)).FirstOrDefault(e => e is not null);
        }

        /// <summary>Writes the files and appends an image row for each, after the existing ones. Returns the stored names.</summary>
        private async Task<List<string>> SaveImagesAsync(Property property, IReadOnlyList<FileUpload> images)
        {
            var saved = new List<string>();
            var order = property.Images.Count == 0 ? 0 : property.Images.Max(i => i.DisplayOrder) + 1;

            try
            {
                foreach (var image in images)
                {
                    // Only the extension of the uploaded name is kept: FileStorageService prefixes a GUID, and
                    // dropping the original name keeps spaces / Arabic characters out of the URL.
                    var stored = await _fileStorage.SaveFileAsync(
                        image.Content, Path.GetExtension(image.FileName).ToLowerInvariant(), PropertyImageRules.Folder);
                    saved.Add(stored);

                    property.Images.Add(new PropertyImage
                    {
                        ImageUrl = PropertyImageRules.Url(stored),
                        DisplayOrder = order++,
                    });
                }
            }
            catch
            {
                DeleteFiles(saved);
                throw;
            }

            return saved;
        }

        private void DeleteFiles(IEnumerable<string> storedNames)
        {
            foreach (var name in storedNames)
                _fileStorage.DeleteFile(name, PropertyImageRules.Folder);
        }

        /// <summary>Exactly one cover whenever there are images: keep the first marked one, else the first in order.</summary>
        private static void EnsureSingleCover(IEnumerable<PropertyImage> images)
        {
            var ordered = images.OrderBy(i => i.DisplayOrder).ToList();
            if (ordered.Count == 0)
                return;

            var cover = ordered.FirstOrDefault(i => i.IsCover) ?? ordered[0];
            foreach (var image in ordered)
                image.IsCover = ReferenceEquals(image, cover);
        }

        private static void Renumber(IList<PropertyImage> images)
        {
            for (var i = 0; i < images.Count; i++)
                images[i].DisplayOrder = i;
        }

        private async Task<List<PropertyImage>> GetImagesInScopeAsync(int propertyId, string? ownerScope) =>
            (await _unitOfWork.Repository<PropertyImage>().GetAllAsync(new PropertyImagesInScopeSpec(propertyId, ownerScope))).ToList();
    }
}
