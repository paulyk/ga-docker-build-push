namespace SKD.Server;

[ExtendObjectType<Query>]
public class SummaryQuery {

    public async Task<IEnumerable<ItemCountDTO>> KitsByTimelineStatusSummary(
        [Service] SummaryQueryService service
    ) => await service.KitsByTimelineStatusSummary();

}

