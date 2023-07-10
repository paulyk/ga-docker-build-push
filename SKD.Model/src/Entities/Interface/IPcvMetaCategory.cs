namespace SKD.Model;

public interface IPcvMetaCategory : ICategory {
    public ICollection<PCV> Pcvs { get; set; }
}