using System.Reflection;
using Maqar.Domain.Models;
using Maqar.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Maqar.Repository.Data
{
    public class MaqarContext : IdentityDbContext<AppUser>
    {
        public MaqarContext(DbContextOptions<MaqarContext> options) : base(options)
        {
            // Cascade at SaveChanges rather than at Remove(): by then ApplySoftDeletes has turned a
            // soft-deletable parent back into a Modified row, so its tracked images and feature links
            // are never cascade-deleted along with it.
            ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<PropertyEvent> PropertyEvents { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyAuditAndSoftDelete()
        {
            var now = DateTime.UtcNow;

            // ToList: the loop changes entry states, which must not happen mid-enumeration.
            foreach (var entry in ChangeTracker.Entries().ToList())
            {
                if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    softDeletable.IsDeleted = true;
                    softDeletable.DeletedAt = now;
                }

                if (entry.Entity is IAuditableEntity auditable)
                    StampAudit(entry, auditable, now);
            }
        }

        private static void StampAudit(EntityEntry entry, IAuditableEntity auditable, DateTime now)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    auditable.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    auditable.UpdatedAt = now;
                    // A detached entity passed to Update() arrives with CreatedAt unset — never overwrite it.
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                    break;
            }
        }
    }
}
