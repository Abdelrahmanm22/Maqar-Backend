using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maqar.Domain.Models;

namespace Maqar.Repository.Data.Configrations
{
    public class AreaConfig : IEntityTypeConfiguration<Area>
    {
        public void Configure(EntityTypeBuilder<Area> builder)
        {
            builder.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.NameAr)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.NameEn)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.Key).IsUnique();

            // Keys mirror AreaKey in the frontend (src/data/properties.ts) — renaming one breaks its filters.
            builder.HasData(
                new Area { Id = 1, Key = "fifthSettlement", NameAr = "التجمع الخامس", NameEn = "Fifth Settlement", DisplayOrder = 1 },
                new Area { Id = 2, Key = "maadi", NameAr = "المعادي", NameEn = "Maadi", DisplayOrder = 2 },
                new Area { Id = 3, Key = "sheikhZayed", NameAr = "الشيخ زايد", NameEn = "Sheikh Zayed", DisplayOrder = 3 },
                new Area { Id = 4, Key = "nasrCity", NameAr = "مدينة نصر", NameEn = "Nasr City", DisplayOrder = 4 },
                new Area { Id = 5, Key = "october", NameAr = "6 أكتوبر", NameEn = "6th of October", DisplayOrder = 5 });
        }
    }
}
