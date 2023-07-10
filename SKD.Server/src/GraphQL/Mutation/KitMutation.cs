namespace SKD.Server;


[ExtendObjectType<Mutation>]
public class KitMutation {


    /// <summary>
    /// Create a timeline event for a kit
    /// </summary>
    /// <param name="input"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<KitTimelineEvent>> CreateKitTimelineEventAsync(
        [Service] KitService service,
        KitTimelineEventInput input
    ) => await service.CreateKitTimelineEventAsync(input);


    /// <summary>
    /// Create build start event for a kit
    /// </summary>
    /// <param name="kitNo"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<KitTimelineEvent>> CreateBuildStartEventAsync(
        [Service] KitService service,
        string kitNo
    ) => await service.CreateBuildStartEventAsync(kitNo);


    /// <summary>
    /// Create csutom received event for all kits in a lot
    /// </summary>
    /// <param name="input"></param>
    /// <param name="service"></param>
    /// <returns></returns>

    public async Task<SKD.Service.MutationResult<Lot>> CreateLotTimelineEvent(
        [Service] KitService service,
        LotTimelineEventInput input
    ) => await service.CreateLotTimelineEventAsync(input);

    /// <summary>
    /// Update kit component stations mappings to match the component station mappings template
    /// where kit does not have a BUILD_COMPLETE timeline event
    /// </summary>
    public async Task<SKD.Service.MutationResult<SyncKitComponentsPayload>> SyncKitsWithComponentStationMappings(
        [Service] ComponentStationService service
    ) => await service.SyncKitsWithComponentStationMappings();

}