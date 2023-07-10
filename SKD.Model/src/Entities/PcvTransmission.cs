#nullable enable
namespace SKD.Model;

public class PcvTransmission : EntityBase, IPcvMetaCategory {
    public String Code { get; set; } = "";
    public String Name { get; set; } = "";

    public ICollection<PCV> Pcvs { get; set; } = new List<PCV>();
}