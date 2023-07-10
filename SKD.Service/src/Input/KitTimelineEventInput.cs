#nullable enable
namespace SKD.Service;

public class KitTimelineEventInput {
    public string KitNo { get; set; } = "";
    public KitTimelineCode EventCode { get; set; }
    public DateTime EventDate { get; set; }
    public string EventNote { get; set; } = "";
    public string DealerCode { get; set; } = "";
}

