#nullable enable

namespace SKD.Service;

public class CreateKitTimelineEventPayload {
    public CreateKitTimelineEventPayload(string kitNo, KitTimelineCode eventCode, DateTime eventDate) {
        this.KitNo = kitNo;
        this.EventCode = eventCode;
        this.EventDate = eventDate;
    }
    public string KitNo { get; set; } = "";
    public KitTimelineCode EventCode { get; set; }
    public DateTime EventDate { get; set;  }
}