namespace SKD.Server;

public class KitStatusDTO {
    public string VIN { get; set; } = "";
    public string LotNo { get; set; } = "";
    public string KitNo { get; set; } = "";
    public ICollection<StatusEventDTO> TimelineItems { get; set; } = new List<StatusEventDTO>();
}
