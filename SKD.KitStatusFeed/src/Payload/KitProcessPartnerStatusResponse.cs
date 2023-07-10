namespace SKD.KitStatusFeed;

public class KitProcessPartnerStatusResponse {
    public PartnerStatusLayoutData PartnerStatusLayoutData { get; set; } = new PartnerStatusLayoutData();
}

public class PartnerStatusLayoutData {
    public string PartnerGsdb { get; set; } = "";
    public string KdPlantGsdb { get; set; } = "";
    public string AckStatus { get; set; } = "";
    public string ErrorType { get; set; } = "";
    public string ErrorReason { get; set; } = "";
    public string CurrentDate { get; set; } = "";
    public string KitNumber { get; set; } = "";
    public string LotNumber { get; set; } = "";
}
