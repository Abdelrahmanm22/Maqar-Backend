using AutoMapper;
using Maqar.Admin.ViewModels;
using Maqar.Domain.Models;

namespace Maqar.Admin.MappingProfiles
{
    public class LookupProfile : Profile
    {
        public LookupProfile()
        {
            CreateMap<Area, AreaViewModel>()
                .ForMember(d => d.PropertyCount, o => o.Ignore());
            CreateMap<AreaViewModel, Area>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Properties, o => o.Ignore());

            CreateMap<Feature, FeatureViewModel>()
                .ForMember(d => d.PropertyCount, o => o.Ignore());
            CreateMap<FeatureViewModel, Feature>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Properties, o => o.Ignore())
                .ForMember(d => d.Icon, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Icon) ? null : s.Icon.Trim()));
        }
    }
}
