#nullable enable

namespace SKD.Model;

public partial class PCV : EntityBase {
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelYear { get; set; } = "";
    public string Model { get; set; } = "";
    public string Series { get; set; } = "";
    public string Body { get; set; } = "";

    public PcvModel? PcvModel { get; set; } 
    public Guid? PcvModelId { get; set; }

    public PcvSubmodel? PcvSubmodel { get; set; } 
    public Guid? PcvSubmodelId { get; set; }

    public PcvSeries? PcvSeries { get; set; } 
    public Guid? PcvSeriesId { get; set; }

    public PcvEngine? PcvEngine { get; set; } 
    public Guid? PcvEngineId { get; set; }

    public PcvTransmission? PcvTransmission { get; set; } 
    public Guid? PcvTransmissionId { get; set; }

    public PcvDrive? PcvDrive { get; set; } 
    public Guid? PcvDriveId { get; set; }

    public PcvPaint? PcvPaint { get; set; } 
    public Guid? PcvPaintId { get; set; }
    
    public PcvTrim? PcvTrim { get; set; } 
    public Guid? PcvTrimId { get; set; }

    public ICollection<Lot> Lots { get; set; } = new List<Lot>();
    public ICollection<PcvComponent> PcvComponents { get; set; } = new List<PcvComponent>();
}
