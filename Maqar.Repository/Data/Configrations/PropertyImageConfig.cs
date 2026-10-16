using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maqar.Domain.Models;

namespace Maqar.Repository.Data.Configrations
{
    public class PropertyImageConfig : IEntityTypeConfiguration<PropertyImage>
    {
        public void Configure(EntityTypeBuilder<PropertyImage> builder)
        {
            builder.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(x => x.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.PropertyId, x.DisplayOrder });

            // Matches Property's soft-delete filter, so a required navigation never points at a hidden row.
            builder.HasQueryFilter(x => !x.Property.IsDeleted);
        }
    }
}
