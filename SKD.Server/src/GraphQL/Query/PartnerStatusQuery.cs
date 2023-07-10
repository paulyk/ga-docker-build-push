using SKD.KitStatusFeed;

namespace SKD.Server;

[ExtendObjectType<Query>]
public class PartnerStatusQuery {

    /// <summary>
    /// Get kit current status from Kit Status Feed api
    /// </summary>
    /// <param name="service"></param>
    /// <param name="kitNo"></param>
    /// <returns></returns>
    public async Task<KitCurrentStatusResponse> GetPartnerKitCurrentStatusAsync(
        [Service] PartnerStatusService service,
        string kitNo
    ) {
        return await service.GetKitCurrentStatusAsync(new KitCurrentStatusRequest {
            KitNumber = kitNo
        });
    }

    public async Task<PvinFeedLayoutData> GetPartnerKitPhysicalVinAsync(
        SkdContext context,
        [Service] PartnerStatusService service,
        string kitNo
    ) {
        var reuslt = await service.GetPvinAsync(kitNo);

        return reuslt;
    }
}