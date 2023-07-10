#nullable enable

namespace SKD.Service;

public class SummaryQueryService {
    private readonly SkdContext context;

    public SummaryQueryService(SkdContext ctx) {
        this.context = ctx;
    }

    /// <summary>
    /// Return kits where latest timline event matches provide timelineEventCode
    /// </summary>
    /// <param name="timelineEventCode"></param>
    /// <returns></returns>
    public IQueryable<Kit> KitsByCurrentTimelineStatus(
        KitTimelineCode timelineEventCode
    ) => context.Kits.AsNoTracking()
        .Where(t => t.RemovedAt == null)
        .Where(t =>
            t.TimelineEvents
                .OrderByDescending(t => t.EventType.Sequence)
                .Where(t => t.RemovedAt == null)
                .Select(x => x.EventType.Code)
                .FirstOrDefault() == timelineEventCode).AsQueryable();

    /// <summary>
    /// Group count of all kits by current timeline status
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ItemCountDTO>> KitsByTimelineStatusSummary() {

        var groupedKitsByCurrentTimelineStatus = await context.Kits.AsNoTracking()
            .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.TimelineEvents.Where(t => t.RemovedAt == null).Any())
            .Select(t => new {
                KitNo = t.KitNo,
                Event = t.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.EventType.Sequence)
                .First()
            })
            .Select(t => new {
                EventType = t.Event.EventType
            })
            .GroupBy(t => new { 
                t.EventType.Code, 
                t.EventType.Description, 
                t.EventType.Sequence })
            .Select(g => new {
                Code = g.Key.Code,
                Name = g.Key.Description,
                Sequence = g.Key.Sequence,
                Count = g.Count()
            })
            .OrderBy(t => t.Sequence)
            .Select(t => new ItemCountDTO {
                Code = t.Code.ToString(),
                Name = t.Name,
                Count = t.Count
            })
            .ToListAsync();

        var kitTimelineEventTypes = await context.KitTimelineEventTypes.AsNoTracking()
            .OrderBy(t => t.Sequence).Where(t => t.RemovedAt == null)
            .Select(t => new ItemCountDTO {
                Code = t.Code.ToString(),
                Name = t.Description,
                Count = 0
            }).ToListAsync();

        kitTimelineEventTypes.ForEach(t => {
            var existing = Enumerable.FirstOrDefault<ItemCountDTO>((IEnumerable<ItemCountDTO>)groupedKitsByCurrentTimelineStatus, (Func<ItemCountDTO, bool>)(x => x.Code == t.Code));
            if (existing != null) {
                t.Count = existing.Count;
            }
        });

        var noTimelineEventsKitCount = await context.Kits
            .Where(t => t.RemovedAt == null)
            .Where(t => !t.TimelineEvents.Any(t => t.RemovedAt == null))
            .CountAsync();

        if (noTimelineEventsKitCount > 0) {
            kitTimelineEventTypes.Insert(0, new ItemCountDTO {
                Code = "",
                Name = "Custom receive pending",
                Count = noTimelineEventsKitCount
            });
        }

        return kitTimelineEventTypes;
    }
}
