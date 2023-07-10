#nullable enable

namespace SKD.Service;
public class GenerateBuildStartEventsPayload {
    public string PlantCode { get; set; } = null!;
    public List<string> KitNos { get; set; } = new();
}