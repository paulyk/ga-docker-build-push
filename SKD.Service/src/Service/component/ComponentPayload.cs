#nullable enable
namespace SKD.Service;

public class ComponentInput {
    public Guid? Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool? DcwsRequired { get; set; }

    ///<summary>Leave empty to remove</summary>
    public string? ProductionStationCode { get; set; }
    public ComponentSerialRule DcwsSerialCaptureRule { get; set; }
}

public class SetDefaultStationInput {
    public string ComponentCode { get; set; } = "";
    public string ProductionStationCode { get; set; } = "";
}

public class UpdateComponentPayload {

    public UpdateComponentPayload(Component component) {
        Id = component.Id;
        Code = component.Code;
        Name = component.Name;        
        ProductionStationCode = component.ProductionStation?.Code;
        DcwsRequired = component.DcwsRequired;
        CreatedAt = component.CreatedAt;
        RemovedAt = component.RemovedAt;
    }

    public Guid Id { get; set;  }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ProductionStationCode { get; set; } = "";
    public bool DcwsRequired { get; set;  }
    public DateTime CreatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
}