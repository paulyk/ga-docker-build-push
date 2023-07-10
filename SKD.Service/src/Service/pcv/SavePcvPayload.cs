#nullable enable
namespace SKD.Service;

public class SavePcvPayload {

    public SavePcvPayload(PCV pcv) {
        Id = pcv.Id;
        PcvCode = pcv.Code;
        ModelYear = pcv.ModelYear;

        PcvModel = pcv.PcvModel;
        PcvSubmodel = pcv.PcvSubmodel;
        PcvSeries = pcv.PcvSeries;
        PcvEngine = pcv.PcvEngine;
        PcvTransmission = pcv.PcvTransmission;
        PcvDrive = pcv.PcvDrive;
        PcvPaint = pcv.PcvPaint;
        PcvTrim = pcv.PcvTrim;

        PcvComponentCodes = pcv.PcvComponents.Select(t => t.Component.Code).ToList();
    }
    public Guid Id { get; set; }
    public string PcvCode { get; set; } = "";
    public string ModelYear { get; set; } = "";

    public ICategory? PcvModel { get; set; }
    public ICategory? PcvSubmodel { get; set; }
    public ICategory? PcvSeries { get; set; }
    public ICategory? PcvEngine { get; set; }
    public ICategory? PcvTransmission { get; set; }
    public ICategory? PcvDrive { get; set; }
    public ICategory? PcvPaint { get; set; }
    public ICategory? PcvTrim { get; set; }

    public ICollection<string> PcvComponentCodes { get; set; } = new List<string>();

}