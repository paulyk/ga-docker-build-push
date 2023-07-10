#nullable enable

using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace SKD.Service;

public class UpdateStationPayload {

    public UpdateStationPayload(ProductionStation station) {
        Id = station.Id;
        Code = station.Code;
        Name = station.Name;
        Sequence = station.Sequence;
        CreatedAt = station.CreatedAt;
        RemovedAt = station.RemovedAt;
    }

    public Guid? Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int Sequence { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

}