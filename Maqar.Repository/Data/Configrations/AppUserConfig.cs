using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maqar.Domain.Models.Identity;

namespace Maqar.Repository.Data.Configrations
{
    public class AppUserConfig : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Gender)
                .IsRequired();

            // No HasDefaultValue(true): EF treats an explicit `false` as "unset" and lets the DB default
            // win, so an inactive user would be saved active. The C# initialiser supplies the default.
            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.WhatsAppNumber)
                .IsRequired(false)
                .HasMaxLength(30);

            builder.Property(x => x.ImageUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.HasIndex(x => x.UserName).IsUnique();
            builder.HasIndex(x => x.Email).IsUnique();
        }
    }
}
