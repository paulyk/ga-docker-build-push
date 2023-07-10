namespace SKD.KitStatusFeed;

public class KitPVinResponse {
    public PvinFeedLayoutData PvinFeedLayoutData { get; set; } = new PvinFeedLayoutData();
}

public class PvinFeedLayoutData {
    public string partnerGsdb { get; set; } = "";
    public string kdPlantGsdb { get; set; } = "";
    public string kitNumber { get; set; } = "";
    public string buildDate { get; set; } = "";
    public string lotNumber { get; set; } = "";
    public string? physicalVin { get; set; } 
}
