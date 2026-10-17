using AutoMapper;
using Maqar.Admin.ViewModels;
using Maqar.Domain.Models;

namespace Maqar.Admin.MappingProfiles
{
    public class SettingsProfile : Profile
    {
        public SettingsProfile()
        {
            CreateMap<SiteSettings, SiteSettingsViewModel>();

            // Empty optional fields are stored as null, not "".
            CreateMap<SiteSettingsViewModel, SiteSettings>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.AddressAr, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.AddressAr) ? null : s.AddressAr.Trim()))
                .ForMember(d => d.AddressEn, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.AddressEn) ? null : s.AddressEn.Trim()))
                .ForMember(d => d.FacebookUrl, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.FacebookUrl) ? null : s.FacebookUrl.Trim()))
                .ForMember(d => d.InstagramUrl, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.InstagramUrl) ? null : s.InstagramUrl.Trim()));
        }
    }
}
