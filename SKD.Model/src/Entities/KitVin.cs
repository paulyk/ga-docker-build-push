namespace SKD.Model;

public class KitVin : EntityBase {
    public Guid KitId { get; set; }
    public Kit Kit { get; set; }

    public string VIN { get; set; }
}
