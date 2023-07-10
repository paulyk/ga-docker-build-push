namespace SKD.Service;

public class ParsePcvsXlxsResult {
    public List<PcvDataFromXlsx> PcvData { get; set; } = new List<PcvDataFromXlsx>();
    public List<string> ComponentCodes { get; set; } = new List<string>();
}

public class PcvDataFromXlsx {
    public string PCV { get; set; } = "";
    public string Model { get; set; } = "";
    public string Submodel { get; set; } = "";
    public string Year { get; set; } = "";
    public string Series { get; set; } = "";
    public string Engine { get; set; } = "";
    public string Transmission { get; set; } = "";
    public string Drive { get; set; } = "";
    public string Paint { get; set; } = "";
    public string Trim { get; set; } = "";

    public bool Exists { get; set;  }

    public ICollection<string> ComponentCodes { get; set; } = new List<string>();
}