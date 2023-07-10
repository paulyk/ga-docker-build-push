#nullable enable
namespace SDK.Service;

public class KitInfoDTO {
    public Guid Id { get; private set; }
    public string KitNo { get; private set; } = null!;
    public string LotNo { get; private set; } = null!;
    public string? VIN { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public string Series { get; private set; } = null!;
    public KitTimelineCode? KitTimelineCode { get; private set; }
    public DateTime? EventDtate { get; private set; }
    public PartnerStatusCode? PartnerStatusCode { get; private set; }
    public bool PartnerStatusPending { get; private set; }

    public static KitInfoDTO Create(Kit kit) {

        if (kit.TimelineEvents == null) {
            throw new Exception("kit.TimelineEvents is null");
        }
        if (kit.Lot?.Pcv == null) {
            throw new Exception("kit.Lot.Pcv is null");
        }
        if (kit.TimelineEvents.Any() && kit.TimelineEvents.First().EventType == null) {
            throw new Exception("kit.TimelineEvents.First().EventType is null");
        }

        return new KitInfoDTO {
            Id = kit.Id,
            KitNo = kit.KitNo,
            VIN = kit.VIN,
            LotNo = kit.Lot.LotNo,
            Model = kit.Lot.Pcv.Model,
            Series = kit.Lot.Pcv.Series,
            EventDtate = kit.TimelineEvents.Any()
                ? kit.TimelineEvents
                    .Where(e => e.RemovedAt == null)
                    .OrderByDescending(e => e.EventType.Sequence)
                    .Select(e => e.EventDate)
                    .FirstOrDefault()
                : null,
            KitTimelineCode = kit.TimelineEvents.Any()
                ? kit.TimelineEvents
                    .Where(e => e.RemovedAt == null)
                    .OrderByDescending(e => e.EventType.Sequence)
                    .Select(e => e.EventType.Code)
                    .FirstOrDefault()
                : null,
            PartnerStatusCode = kit.TimelineEvents.Any()
                ? kit.TimelineEvents
                    .Where(e => e.RemovedAt == null)
                    .OrderByDescending(e => e.EventType.Sequence)
                    .Select(e => e.EventType.PartnerStatusCode)
                    .FirstOrDefault()
                : null,
            PartnerStatusPending = kit.TimelineEvents.Any()
                ? kit.TimelineEvents
                    .Where(e => e.RemovedAt == null)
                    .OrderByDescending(e => e.EventType.Sequence)
                    .Select(e => e.PartnerStatusUpdatedAt == null)
                    .FirstOrDefault()
                : false
        };
    }
}