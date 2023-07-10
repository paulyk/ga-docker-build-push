#nullable enable

namespace SKD.Model;

public partial class KitTimelineEventType : EntityBase {
    public KitTimelineCode Code { get; set; }
    public PartnerStatusCode PartnerStatusCode { get; set; }
    public string Description { get; set; } = "";
    public int Sequence { get; set; }

    public ICollection<KitSnapshot> Snapshots { get; set; } = new List<KitSnapshot>();
}
