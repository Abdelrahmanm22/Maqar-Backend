using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Maqar.Admin.ViewModels;
using Maqar.Domain;
using Maqar.Domain.Models;
using Maqar.Domain.Models.Identity;
using Maqar.Domain.Services.Lookups;
using Maqar.Domain.Services.Properties;
using Maqar.Domain.Specifications.ContactMessages;
using Maqar.Domain.Specifications.Properties;

namespace Maqar.Admin.Services
{
    /// <summary>
    /// Builds the Property screens' view models — the lookups, dropdowns and related data a page needs —
    /// so PropertyController only resolves the scope, calls a service and returns a result. Lives in the web
    /// project because the view models (SelectListItem, IFormFile) are web types.
    /// </summary>
    public interface IPropertyViewModelBuilder
    {
        Task<PropertyListViewModel> BuildListAsync(PropertySpecParams filters, string? ownerScope);

        Task<PropertyDetailsViewModel> BuildDetailsAsync(Property property, bool canViewInquiries);

        Task<PropertyFormViewModel> BuildCreateFormAsync();

        Task<PropertyFormViewModel> BuildEditFormAsync(Property property);

        /// <summary>
        /// Re-attaches what a POST doesn't carry — lookups, and on Edit the existing images — before the form
        /// is redisplayed with its errors. <paramref name="existing"/> is null on Create.
        /// </summary>
        Task<PropertyFormViewModel> RefillFormAsync(PropertyFormViewModel model, Property? existing);

        PropertyImageStripViewModel BuildImageStrip(Property property);
    }

    public class PropertyViewModelBuilder : IPropertyViewModelBuilder
    {
        private readonly IPropertyService _propertyService;
        private readonly ILookupService<Area> _areas;
        private readonly ILookupService<Feature> _features;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public PropertyViewModelBuilder(
            IPropertyService propertyService,
            ILookupService<Area> areas,
            ILookupService<Feature> features,
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IMapper mapper)
        {
            _propertyService = propertyService;
            _areas = areas;
            _features = features;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<PropertyListViewModel> BuildListAsync(PropertySpecParams filters, string? ownerScope)
        {
            var vm = new PropertyListViewModel
            {
                Filters = filters,
                Page = await _propertyService.GetPagedAsync(filters, ownerScope),
                CanViewAll = ownerScope is null,
                Areas = (await _areas.GetAllAsync())
                    .Select(a => new SelectListItem(a.NameEn, a.Id.ToString()))
                    .ToList(),
            };

            // The "Added by" filter only exists for unscoped users — a broker is pinned to themselves.
            if (vm.CanViewAll)
            {
                vm.Brokers = await _userManager.Users
                    .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                    .Select(u => new SelectListItem((u.FirstName + " " + u.LastName).Trim(), u.Id))
                    .ToListAsync();
            }

            return vm;
        }

        public async Task<PropertyDetailsViewModel> BuildDetailsAsync(Property property, bool canViewInquiries) => new()
        {
            Property = property,
            CanViewInquiries = canViewInquiries,
            // The property is already scope-checked by the caller, so its own inquiries are fair game.
            Inquiries = canViewInquiries
                ? (await _unitOfWork.Repository<ContactMessage>().GetAllAsync(new ContactMessagesForPropertySpec(property.Id))).ToList()
                : Array.Empty<ContactMessage>(),
        };

        public Task<PropertyFormViewModel> BuildCreateFormAsync() =>
            FillLookupsAsync(new PropertyFormViewModel(), currentAreaId: null);

        public Task<PropertyFormViewModel> BuildEditFormAsync(Property property) =>
            FillLookupsAsync(_mapper.Map<PropertyFormViewModel>(property), property.AreaId);

        public Task<PropertyFormViewModel> RefillFormAsync(PropertyFormViewModel model, Property? existing)
        {
            if (existing is not null)
                model.ExistingImages = existing.Images.OrderBy(i => i.DisplayOrder).ToList();

            return FillLookupsAsync(model, existing?.AreaId);
        }

        public PropertyImageStripViewModel BuildImageStrip(Property property) => new()
        {
            PropertyId = property.Id,
            Images = property.Images.OrderBy(i => i.DisplayOrder).ToList(),
        };

        /// <summary>
        /// Active areas and features only — plus the listing's own area and features even if they have since
        /// been deactivated, so editing an older listing never silently drops them.
        /// </summary>
        private async Task<PropertyFormViewModel> FillLookupsAsync(PropertyFormViewModel model, int? currentAreaId)
        {
            model.Areas = (await _areas.GetAllAsync())
                .Where(a => a.IsActive || a.Id == currentAreaId)
                .Select(a => new SelectListItem(a.IsActive ? a.NameEn : $"{a.NameEn} (inactive)", a.Id.ToString()))
                .ToList();

            model.Features = (await _features.GetAllAsync())
                .Where(f => f.IsActive || model.SelectedFeatureIds.Contains(f.Id))
                .ToList();

            return model;
        }
    }
}
