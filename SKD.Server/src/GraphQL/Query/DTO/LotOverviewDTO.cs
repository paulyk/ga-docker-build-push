namespace SKD.Server;

public class LotOverviewDTO {
    public Guid Id { get; set; }

    public Guid BomId { get; set; }
    public int BomSequence { get; set; }
    public Guid ShipmentId { get; set; }
    public int ShipmentSequence { get; set; }
    public string LotNo { get; set; } = "";
    public string Note { get; set; } = "";
    public string PlantCode { get; set; } = "";
    public Guid PcvId { get; set;  }
    public string PcvCode { get; set; } = "";
    public string PcvDescription { get; set; } = "";
    public StatusEventDTO? CustomReceived { get; set; }
    public DateTime CreatedAt { get; set; }
}
