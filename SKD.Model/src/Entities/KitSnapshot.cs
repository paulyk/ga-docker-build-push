namespace SKD.Model;

public enum SnapshotChangeStatus {
    Added,
    Changed,
    NoChange,
    Final
}


public class KitSnapshot : EntityBase {

    public Guid KitSnapshotRunId { get; set; }
    public KitSnapshotRun KitSnapshotRun { get; set; }
    public Guid KitId { get; set; }
    public Kit Kit { get; set; }
    public SnapshotChangeStatus ChangeStatusCode { get; set; }

    public Guid KitTimeLineEventTypeId { get; set; }
    public KitTimelineEventType KitTimelineEventType { get; set; }

    public string VIN { get; set; }
    public string DealerCode { get; set; }
    public string EngineSerialNumber { get; set; }

    public DateTime? OrginalPlanBuild { get; set; }
    
    public DateTime? CustomReceived { get; set; }
    public DateTime? PlanBuild { get; set; }
    public DateTime? VerifyVIN { get; set; }
    public DateTime? BuildCompleted { get; set; }
    public DateTime? GateRelease { get; set; }
    public DateTime? Wholesale { get; set; }
}