using Microsoft.EntityFrameworkCore;
using Maqar.Domain.Enums;
using Maqar.Domain.Models;
using Maqar.Domain.Repositories;
using Maqar.Domain.Services.Analytics;
using Maqar.Repository.Data;

namespace Maqar.Repository.Repositories
{
    public class PropertyStatsRepository : IPropertyStatsRepository
    {
        private readonly MaqarContext _context;

        public PropertyStatsRepository(MaqarContext context) => _context = context;

        public async Task<PropertyBounds> GetPublishedBoundsAsync()
        {
            var published = _context.Properties.AsNoTracking().Where(p => p.Status == PropertyStatus.Published);

            // One row per listing type: min/max price and size.
            var rows = await published
                .GroupBy(p => p.ListingType)
                .Select(g => new
                {
                    ListingType = g.Key,
                    MinPrice = g.Min(p => p.Price),
                    MaxPrice = g.Max(p => p.Price),
                    MinSize = g.Min(p => p.Size),
                    MaxSize = g.Max(p => p.Size),
                })
                .ToListAsync();

            ValueRange? PriceFor(ListingType type) =>
                rows.FirstOrDefault(r => r.ListingType == type) is { } r ? new(r.MinPrice, r.MaxPrice) : null;

            var size = rows.Count == 0 ? null : new ValueRange(rows.Min(r => r.MinSize), rows.Max(r => r.MaxSize));

            return new PropertyBounds(PriceFor(ListingType.Sale), PriceFor(ListingType.Rent), size);
        }

        public async Task<EventTotals> GetEventTotalsAsync(DateTime? since, string? ownerScope, int? propertyId = null)
        {
            var counts = await ScopedEvents(since, ownerScope)
                .Where(e => propertyId == null || e.PropertyId == propertyId)
                .GroupBy(e => e.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            int CountOf(PropertyEventType type) => counts.FirstOrDefault(c => c.Type == type)?.Count ?? 0;

            return new EventTotals(
                CountOf(PropertyEventType.View),
                CountOf(PropertyEventType.WhatsAppClick),
                CountOf(PropertyEventType.CallClick));
        }

        public async Task<IReadOnlyList<TopProperty>> GetTopAsync(TopPropertiesQuery query)
        {
            var types = query.Types;

            var top = await ScopedEvents(query.Since, query.OwnerScope)
                .Where(e => types.Contains(e.Type))
                .GroupBy(e => e.PropertyId)
                .Select(g => new { PropertyId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(query.Take)
                .ToListAsync();

            if (top.Count == 0)
                return Array.Empty<TopProperty>();

            var ids = top.Select(t => t.PropertyId).ToList();
            var details = await _context.Properties.AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.TitleEn,
                    Cover = p.Images.OrderByDescending(i => i.IsCover).ThenBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                })
                .ToDictionaryAsync(p => p.Id);

            return top
                .Where(t => details.ContainsKey(t.PropertyId))
                .Select(t => new TopProperty(t.PropertyId, details[t.PropertyId].TitleEn, details[t.PropertyId].Cover, t.Count))
                .ToList();
        }

        /// <summary>Events on live listings, within the ownership scope and date window.</summary>
        private IQueryable<PropertyEvent> ScopedEvents(DateTime? since, string? ownerScope) =>
            _context.PropertyEvents.AsNoTracking()
                .Where(e => since == null || e.CreatedAt >= since)
                .Where(e => ownerScope == null || e.Property.CreatedById == ownerScope);
    }
}
