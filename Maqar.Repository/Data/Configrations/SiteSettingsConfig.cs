using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maqar.Domain.Models;

namespace Maqar.Repository.Data.Configrations
{
    public class SiteSettingsConfig : IEntityTypeConfiguration<SiteSettings>
    {
        public void Configure(EntityTypeBuilder<SiteSettings> builder)
        {
            builder.Property(x => x.Phone).IsRequired().HasMaxLength(30);
            builder.Property(x => x.PhoneDisplay).IsRequired().HasMaxLength(40);
            builder.Property(x => x.WhatsApp).IsRequired().HasMaxLength(30);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
            builder.Property(x => x.AddressAr).HasMaxLength(300);
            builder.Property(x => x.AddressEn).HasMaxLength(300);
            builder.Property(x => x.FacebookUrl).HasMaxLength(300);
            builder.Property(x => x.InstagramUrl).HasMaxLength(300);

            // The singleton row, with the values the frontend hardcoded in src/lib/site.ts.
            builder.HasData(new SiteSettings
            {
                Id = SiteSettings.SingletonId,
                Phone = "+201000000000",
                PhoneDisplay = "+20 100 000 0000",
                WhatsApp = "201000000000",
                Email = "hello@maqar.eg",
            });
        }
    }
}
