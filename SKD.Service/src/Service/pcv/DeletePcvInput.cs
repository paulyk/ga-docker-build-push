namespace SKD.Service;

public class DeletePcvInput {
    public string PcvCode { get; set; }
}

public class DeletePcvPayload {
    public string PcvCode { get; set; }
    public string PcvModel { get; set;  }
}
