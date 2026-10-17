using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maqar.Domain.Models;

namespace Maqar.Repository.Data.Configrations
{
    public class PropertyEventConfig : IEntityTypeConfiguration<PropertyEvent>
    {
        public void Configure(EntityTypeBuilder<PropertyEvent> builder)
        {
            builder.Property(x => x.VisitorHash)
                .IsRequired()
                .HasMaxLength(64);

            builder.HasOne(x => x.Property)
                .WithMany()
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Dashboard stats: counts per listing / type over a date range.
            builder.HasIndex(x => new { x.PropertyId, x.Type, x.CreatedAt });
            // The dedup probe: has this visitor done this to this listing recently?
            builder.HasIndex(x => new { x.VisitorHash, x.PropertyId, x.Type, x.CreatedAt });

            // Matches Property's soft-delete filter (a required navigation must not point at a hidden row).
            builder.HasQueryFilter(x => !x.Property.IsDeleted);
        }
    }
}
