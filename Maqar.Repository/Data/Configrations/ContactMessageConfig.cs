using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maqar.Domain.Models;

namespace Maqar.Repository.Data.Configrations
{
    public class ContactMessageConfig : IEntityTypeConfiguration<ContactMessage>
    {
        public void Configure(EntityTypeBuilder<ContactMessage> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Phone).IsRequired().HasMaxLength(30);
            builder.Property(x => x.Message).IsRequired().HasMaxLength(2000);

            builder.HasOne(x => x.Property)
                .WithMany(p => p.ContactMessages)
                .HasForeignKey(x => x.PropertyId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // The admin inbox: newest first, filtered by status.
            builder.HasIndex(x => new { x.Status, x.CreatedAt });
        }
    }
}
