using System.Text.Json;
using SKD.KitStatusFeed;


namespace SKD.Server;

[ExtendObjectType<Mutation>]
public class PartnerStatusMutation {


    /// <summary>
    /// Given a Kit number update the partner status with the kit's current statuses
    /// Does NOT set the KitTimelineEvent.PartnerStatusUpdatedAt
    /// </summary>
    /// <param name="partnerStatusService"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<UpdatePartnerStatusPayload>> UpdatePartnerStatusAsync(
        [Service] PartnerStatusService partnerStatusService,
        UpdatePartnerStatusInput input
    ) => await partnerStatusService.UpdatePartneStatusAsync(input);

    /// <summary>
    /// Update each KitTimelineEvent.PartnerStatusUpdatedAt based on the partners kit status.
    /// </summary>
    /// <param name="partnerStatusService"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<UpdatePartnerStatusPayload>> SyncKitToPartnerStatusAsync(
        [Service] PartnerStatusService partnerStatusService,
        UpdatePartnerStatusInput input
    ) => await partnerStatusService.SyncKitToPartnerStatusAsync(input);

    /// <summary>
    /// Gets a VIN from the KitStatusFeedService and sets / updates  it to the kit
    /// Does not throw an error if the VIN is not found or if the VIN has not changed
    /// </summary>
    /// <param name="service"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<SKD.Service.MutationResult<UpdateKitVinPayload>> UpdateKitVinAsync    (
        [Service] PartnerStatusService service,
        UpdateKitVinInput input
    ) => await service.UpdateKitVinAsync(input);

    /// <summary>
    /// For every KitSnapshot find the related KitTimelineEvent 
    /// and set the KitTimelineEvent.PartnerStatusUpdatedAt to the KitSnapshot.CreatedAt
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<int> MigrateKitSnapshot(
        SkdContext context
    ) {
        var count = 0;
        var kits = await context.Kits
                .Include(k => k.TimelineEvents.Where(t => t.RemovedAt == null))
                .ThenInclude(k => k.EventType)
            .ToListAsync();

        foreach (var kit in kits) {
            var snapshot = await context.KitSnapshots
                .OrderByDescending(s => s.CreatedAt)
                .Where(s => s.RemovedAt == null)
                .Where(s => s.KitId == kit.Id)
                .FirstOrDefaultAsync();
            Console.WriteLine(kit.KitNo);

            foreach (var kte in kit.TimelineEvents) {
                if (snapshot != null) {
                    kte.PartnerStatusUpdatedAt = snapshot.CreatedAt;
                    count++;
                }
            }
        }

        await context.SaveChangesAsync();
        return count;
    }
}
