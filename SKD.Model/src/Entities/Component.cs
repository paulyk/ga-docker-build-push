#nullable enable
namespace SKD.Model;

public enum ComponentSerialRule {
    ONE_OR_BOTH_SERIALS,
    ONE_SERIAL,
    BOTH_SERIALS,
    VIN_AND_BODY,
    VIN
}

public class Component : EntityBase {
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string IconURL { get; set; } = "";

    public ProductionStation? ProductionStation { get; set; }
    public Guid? ProductionStationId { get; set; }

    public ComponentSerialRule ComponentSerialRule { get; set; }
    public bool DcwsRequired { get; set; }    

    public ICollection<PcvComponent> PcvComponents { get; set; } = new List<PcvComponent>();
    public ICollection<KitComponent> KitComponents { get; set; }= new List<KitComponent>();
    public ICollection<ComponentStation> ComponentStations { get; set;  } = new List<ComponentStation>();

}
