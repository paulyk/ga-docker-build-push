namespace SDK.Service;

public class CustomQueryService {
    private readonly SkdContext _context;

    public CustomQueryService(SkdContext context) {
        _context = context;
    }

    /// <summary>
    /// Gets kits where status is BUILD_START status should be created
    /// Current status is PLAN_BUILD and has component serial input
    /// </summary>
    /// <param name="plantCode"></param>
    /// <returns></returns>
    public async Task<List<KitInfoDTO>> GetBuildStartPendingKits(
        string plantCode
    ) {

        IQueryable<Kit> query = _context.Kits;

        query = query.Include(k => k.Lot).ThenInclude(k => k.Pcv);

        // Include TimelineEvents and EventType
        query = query.Include(k => k.TimelineEvents).ThenInclude(k => k.EventType);

        // Filter by Plant Code
        query = query.Where(k => k.Lot.Plant.Code == plantCode);

        // Filter by Latest event is PLAN_BUILD
        query = query.Where(k =>
            k.TimelineEvents
            .Where(e => e.RemovedAt == null)
            .OrderByDescending(e => e.EventType.Sequence)
            .Select(e => e.EventType.Code)
            .FirstOrDefault() == KitTimelineCode.PLAN_BUILD
        );

        // Filter must have a component serial input
        query = query.Where(k =>
            k.KitComponents
                .Where(kc => kc.RemovedAt == null)
                .SelectMany(kc => kc.ComponentSerials)
                    .Where(c => c.RemovedAt == null)
                    .Any()
        );

        // Select KitInfoDTO
        IQueryable<KitInfoDTO> dtoQuery = query.Select(k => KitInfoDTO.Create(k));

        // Execute the query and get the result
        var result = await dtoQuery.AsNoTracking().ToListAsync();

        return result;
    }


    /// <summary>
    /// Kits that have timeline event entries that have not beed synced to partner status
    /// </summary>
    /// <param name="plantCode"></param>
    /// <returns></returns>
    public async Task<List<KitInfoDTO>> GetUpdatePartnerStatusPendingKits(
        string plantCode
    ) {
        IQueryable<Kit> query = _context.Kits;

        // Include Lot and Pcv
        query = query.Include(k => k.Lot).ThenInclude(k => k.Pcv);

        // Include TimelineEvents (not removed) and EventType
        query = query.Include(k => k.TimelineEvents.Where(te => te.RemovedAt == null)).ThenInclude(k => k.EventType);

        // Filter by Plant Code
        query = query.Where(k => k.Lot.Plant.Code == plantCode);

        // Filter where Latest TimelineEvent's PartnerStatusUpdatedAt is null (not synced to partner)
        query = query.Where(k =>
            k.TimelineEvents
            .Where(e => e.RemovedAt == null)
            .OrderByDescending(e => e.EventType.Sequence)
            .Select(e => e.PartnerStatusUpdatedAt)
            .FirstOrDefault() == null
        );

        // Filter where there's at least one TimelineEvent found that needs to be updaed
        query = query.Where(t => t.TimelineEvents.Any());

        // Select, create KitInfoDTO
        IQueryable<KitInfoDTO> dtoQuery = query.Select(k => KitInfoDTO.Create(k));

        // Execute the query and get the result
        var result = await dtoQuery.AsNoTracking().ToListAsync();

        return result;
    }

    /// <summary>
    /// Gets kits where most recent status is PLAN_BUILD & VIN is empty & PartnerStatusUpdatedAt is not null
    /// </summary>
    /// <param name="plantCode"></param>
    /// <returns></returns>
    public async Task<List<KitInfoDTO>> GetPlanBuildVinPendingKits(
        string plantCode
    ) {

        IQueryable<Kit> query = _context.Kits;

        // Include Lot and Pcv
        query = query.Include(k => k.Lot).ThenInclude(k => k.Pcv);

        // Include TimelineEvents and EventType
        query = query.Include(k => k.TimelineEvents).ThenInclude(k => k.EventType);

        // Filter by Plant Code
        query = query.Where(k => k.Lot.Plant.Code == plantCode);

        // Filter by VIN is empty or null
        query = query.Where(k => k.VIN == "" || k.VIN == null);

        // Filter where Latest TimelineEvent's EventType Code is PLAN_BUILD and PartnerStatusUpdatedAt is not null
        query = query.Where(k =>
            k.TimelineEvents
            .Where(e => e.RemovedAt == null && e.PartnerStatusUpdatedAt != null)
            .OrderByDescending(e => e.EventType.Sequence)
            .Select(e => e.EventType.Code)
            .FirstOrDefault() == KitTimelineCode.PLAN_BUILD
        );

        IQueryable<KitInfoDTO> dtoQuery = query.Select(k => KitInfoDTO.Create(k));

        var result = await dtoQuery.AsNoTracking().ToListAsync();

        return result;
    }

}