#nullable enable
namespace SKD.Model;

public partial class Kit : EntityBase {
    public virtual string VIN { get; set; } = "";
    public string KitNo { get; set; } = "";

    public Guid LotId { get; set; }
    public Lot Lot { get; set; } = new Lot();

    public Guid? DealerId { get; set; }
    public Dealer? Dealer { get; set; } 

    public virtual List<KitComponent> KitComponents { get; set; } = new List<KitComponent>();
    public virtual List<KitTimelineEvent> TimelineEvents { get; set; } = new List<KitTimelineEvent>();
    public virtual List<KitSnapshot> Snapshots { get; set; } = new List<KitSnapshot>();
    public virtual List<KitVin> KitVins { get; set; } = new List<KitVin>();
}
