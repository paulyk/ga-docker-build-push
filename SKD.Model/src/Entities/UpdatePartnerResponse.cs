namespace SKD.Model;

public class UpdatePartnerResponse: EntityBase {

    public Guid KitStatusEventId { get; set; }
    public KitTimelineEvent KitStatusEvent { get; set; } = null!;

    public bool IsSuccess { get; set; }
    public string PartnerGsdb { get; set; } = "";
    public string KdPlantGsdb { get; set; } = "";
    public string AckStatus { get; set; } = "";
    public string ErrorType { get; set; } = "";
    public string ErrorReason { get; set; } = "";
    public string CurrentDate { get; set; } = "";
    public string KitNumber { get; set; } = "";
    public string LotNumber { get; set; } = "";
}