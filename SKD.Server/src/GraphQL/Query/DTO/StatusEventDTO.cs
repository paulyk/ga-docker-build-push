namespace SKD.Server;

public class StatusEventDTO {
    public KitTimelineCode EventType { get; set; }
    public DateTime? EventDate { get; set; }
    public string? EventNote { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    public DateTime? PartnerStatusUpdatedAt { get; set; }
    public int Sequence { get; set; }
}
