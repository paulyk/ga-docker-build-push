namespace SKD.Service;

public class KitPayload {

    public string KitNo { get; set; } = "";
    public string LotNo { get; set; } = "";
    public KitTimelineCode? CurrentEventCode { get; set; }

    public static KitPayload Create(Kit kit) => new KitPayload {
        KitNo = kit.KitNo,
        LotNo = kit.Lot.LotNo,
        CurrentEventCode = kit.TimelineEvents
            .OrderByDescending(t => t.EventType.Sequence)
            .Where(e => e.RemovedAt == null)
            .Select(e  => e.EventType.Code)
            .FirstOrDefault()
    };
}