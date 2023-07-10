namespace SKD.Service;

public class BomListDTO {
    public Guid Id { get; set; }
    public string PlantCode { get; set; } = "";
    public int Sequence { get; set; }
    public string Filename { get; set;  }
    public IEnumerable<BomList_Lot> Lots { get; set; } = new List<BomList_Lot>();
    public DateTime CreatedAt { get; set; }

    public class BomList_Lot {
        public string LotNo { get; set; } = "";
        public int? ShipmentSequence { get; set; }
    }
}
