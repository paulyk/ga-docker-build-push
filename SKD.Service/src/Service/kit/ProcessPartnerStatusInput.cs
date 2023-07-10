namespace SKD.Model;

public class ProcessPartnerStatusInput {
    public string PlantGsdb { get; set; } = "";    
    public string StatusDate { get; set; } = "";  // Format YYYY-MM-DD HH:MM:SS
    public string PartnerGsdb { get; set; } = "";
    public string ActualDealerCode { get; set; } = "";
    public string KitNumber { get; set; } = "";
    public string BuildDate { get; set; } = ""; // Format YYYY-MM-DD
    public string LotNumber { get; set; } = "";
    public string EngineSerialNumber { get; set; } = "";
    public string PhysicalVin { get; set; } = "";
    public string Status { get; set; } = "";

}
