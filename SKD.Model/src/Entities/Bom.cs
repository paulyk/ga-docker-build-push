#nullable enable
namespace SKD.Model;

public class Bom : EntityBase {
    public Guid PlantId { get; set; }
    public Plant Plant { get; set; } = new Plant();
    public int Sequence { get; set; }
    public string? Filename { get; set; }
    public bool LotPartQuantitiesMatchShipment { get; set; }
    public ICollection<Lot> Lots { get; set; } = new List<Lot>();
}
