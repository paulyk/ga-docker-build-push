namespace SKD.Service;

public class ComponentStationMapping {

    public string ComponentCode { get; set; } = "";
    public string StationCode { get; set; } = "";
    public bool SaveCDCComponent { get; set; } = false;
}

public class ComponentStationMappingsInput {
    // mappings
    public List<ComponentStationMapping> Mappings { get; set; } = new List<ComponentStationMapping>();
}

public class SetComponentStationMappingsPayload {
    public List<ComponentStationMapping> Mappings { get; set; } = new List<ComponentStationMapping>();
}