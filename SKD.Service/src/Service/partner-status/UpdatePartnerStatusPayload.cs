namespace SKD.Service;

public class UpdatePartnerStatusPayload {
    public string KitNo { get; set; } = "";    
    public string Message { get; set; } = "";
    public List<PartnerStatusCode> UpdatedStatuses { get; set; } = new();
}