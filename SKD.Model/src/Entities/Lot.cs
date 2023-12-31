namespace SKD.Model;

public partial class Lot : EntityBase {
    public string LotNo { get; set; } = "";
    public string Note { get; set; } = "";

    public Guid PlantId { get; set; }
    public virtual Plant Plant { get; set; }

    public Guid? BomId { get; set; }
    public virtual Bom Bom { get; set; }

    public Guid PcvId { get; set; }
    public virtual PCV Pcv { get; set; }

    public List<Kit> Kits { get; set; } = new List<Kit>();
    public List<LotPart> LotParts { get; set; } = new List<LotPart>();
    public List<ShipmentLot> ShipmentLots { get; set; } = new List<ShipmentLot>();
}
