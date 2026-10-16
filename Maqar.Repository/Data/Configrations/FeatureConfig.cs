using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maqar.Domain.Models;

namespace Maqar.Repository.Data.Configrations
{
    public class FeatureConfig : IEntityTypeConfiguration<Feature>
    {
        public void Configure(EntityTypeBuilder<Feature> builder)
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

            builder.Property(x => x.Icon)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.HasIndex(x => x.Key).IsUnique();

            // Keys mirror FeatureKey in the frontend; labels come from its src/locales/{ar,en}.ts.
            builder.HasData(
                new Feature { Id = 1, Key = "elevator", NameAr = "مصعد", NameEn = "Elevator", DisplayOrder = 1 },
                new Feature { Id = 2, Key = "security", NameAr = "حارس أمن", NameEn = "Security", DisplayOrder = 2 },
                new Feature { Id = 3, Key = "garage", NameAr = "جراج", NameEn = "Garage", DisplayOrder = 3 },
                new Feature { Id = 4, Key = "garden", NameAr = "حديقة", NameEn = "Garden", DisplayOrder = 4 },
                new Feature { Id = 5, Key = "ac", NameAr = "تكييف مركزي", NameEn = "Central AC", DisplayOrder = 5 },
                new Feature { Id = 6, Key = "pool", NameAr = "حمام سباحة", NameEn = "Swimming pool", DisplayOrder = 6 },
                new Feature { Id = 7, Key = "balcony", NameAr = "بلكونة", NameEn = "Balcony", DisplayOrder = 7 },
                new Feature { Id = 8, Key = "kitchen", NameAr = "مطبخ مجهز", NameEn = "Fitted kitchen", DisplayOrder = 8 });
        }
    }
}
