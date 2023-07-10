#nullable enable

namespace SKD.Model;

public class ComponentStation : EntityBase {
    public Component Component { get; set; } = new Component();
    public Guid ComponentId { get; set; }
    public ProductionStation Station { get; set; } = new ProductionStation();
    public Guid StationId { get; set; }

    /// <summary>
    /// Indicates if component code should be submitted to Ford DCWS at this station.
    /// </summary>
    public bool SaveCDCComponent { get; set; }
}