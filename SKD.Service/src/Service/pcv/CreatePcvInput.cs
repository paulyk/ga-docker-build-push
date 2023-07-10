#nullable enable
namespace SKD.Service;

public class SavePcvInput {
    public string PcvCode { get; set; } = "";
    public int ModelYear { get; set; } 

    public string PcvModel { get; set; } = "";
    public string PcvSubmodel { get; set; } = "";
    public string PcvSeries { get; set; } = "";
    public string PcvEngine { get; set; } = "";
    public string PcvTransmission { get; set; } = "";
    public string PcvDrive { get; set; } = "";
    public string PcvPaint { get; set; } = "";
    public string PcvTrim { get; set; } = "";

    public ICollection<string> ComponentCodes { get; set; } = new List<string>();
}

public class CategoryInput : ICategory {
    public CategoryInput(string code, string name) {
        Code = code;
        Name = name;
    }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}