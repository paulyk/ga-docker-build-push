#nullable enable
namespace SKD.Service;

public class StationInput {
    public Guid? Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int Sequence { get; set;  }
}
