using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maqar.Domain.Enums;
using Maqar.Domain.Models;

namespace Maqar.Repository.Data.Configrations
{
    public class PropertyConfig : IEntityTypeConfiguration<Property>
    {
        public void Configure(EntityTypeBuilder<Property> builder)
        {
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Properties_RentalUnit",
                $"[ListingType] = {(int)ListingType.Rent} OR [RentalUnit] IS NULL"));

            builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Size).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Latitude).HasColumnType("decimal(9,6)");
            builder.Property(x => x.Longitude).HasColumnType("decimal(9,6)");

            builder.Property(x => x.TitleAr).IsRequired().HasMaxLength(200);
            builder.Property(x => x.TitleEn).IsRequired().HasMaxLength(200);
            builder.Property(x => x.DescriptionAr).IsRequired();
            builder.Property(x => x.DescriptionEn).IsRequired();
            builder.Property(x => x.LocationAr).IsRequired().HasMaxLength(250);
            builder.Property(x => x.LocationEn).IsRequired().HasMaxLength(250);

            builder.HasOne(x => x.Area)
                .WithMany(a => a.Properties)
                .HasForeignKey(x => x.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Features)
                .WithMany(f => f.Properties)
                .UsingEntity<Dictionary<string, object>>(
                    "PropertyFeatures",
                    r => r.HasOne<Feature>().WithMany().HasForeignKey("FeatureId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Property>().WithMany().HasForeignKey("PropertyId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasKey("PropertyId", "FeatureId"));

            // The public listing query: published, not deleted, filtered by listing type / type / area.
            builder.HasIndex(x => new { x.Status, x.IsDeleted, x.ListingType, x.Type, x.AreaId });
            builder.HasIndex(x => x.Price);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.IsFeatured);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
