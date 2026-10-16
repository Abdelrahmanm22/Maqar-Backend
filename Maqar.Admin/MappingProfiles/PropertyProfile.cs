using AutoMapper;
using Maqar.Admin.ViewModels;
using Maqar.Domain.Models;
using Maqar.Domain.Services.Properties;

namespace Maqar.Admin.MappingProfiles
{
    public class PropertyProfile : Profile
    {
        public PropertyProfile()
        {
            // Entity → form (Edit GET). Lookup lists and images are filled by the controller.
            CreateMap<Property, PropertyFormViewModel>()
                .ForMember(d => d.SelectedFeatureIds, o => o.MapFrom(s => s.Features.Select(f => f.Id)))
                .ForMember(d => d.ExistingImages, o => o.MapFrom(s => s.Images.OrderBy(i => i.DisplayOrder)))
                .ForMember(d => d.NewImages, o => o.Ignore())
                .ForMember(d => d.Areas, o => o.Ignore())
                .ForMember(d => d.Features, o => o.Ignore());

            // Form → service input. Price/Size/AreaId are nullable on the form only so [Required] can fire;
            // validation has already passed by the time this runs.
            CreateMap<PropertyFormViewModel, PropertyInput>()
                .ConstructUsing(s => new PropertyInput(
                    s.Type, s.ListingType, s.RentalUnit,
                    s.Price ?? 0, s.IsNegotiable,
                    s.Bedrooms, s.Bathrooms, s.Size ?? 0, s.Finishing, s.Floor,
                    s.TitleAr, s.TitleEn, s.DescriptionAr, s.DescriptionEn, s.LocationAr, s.LocationEn,
                    s.AreaId ?? 0, s.Latitude, s.Longitude,
                    s.SelectedFeatureIds))
                .ForAllMembers(o => o.Ignore());
        }
    }
}
